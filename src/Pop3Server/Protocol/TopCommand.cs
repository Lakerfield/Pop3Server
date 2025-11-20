using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using Pop3Server.ComponentModel;
using Pop3Server.IO;
using Pop3Server.Storage;

namespace Pop3Server.Protocol
{
  public static class ASCII
  {
    /// <summary>
    /// Carriage return (CR)
    /// </summary>
    public const byte CR = 13;

    /// <summary>
    /// Line feed (LF)
    /// </summary>
    public const byte LF = 10;

    /// <summary>
    /// dot (.)
    /// </summary>
    public const byte Dot = 46;

    public static readonly byte[] DotArray = new byte[] { ASCII.Dot };
    public static readonly byte[] CrLfArray = new byte[] { ASCII.CR, ASCII.LF };
    public const string NewLine = "\r\n";
    public const int NewLineLength = 2;
  }

  public sealed class TopCommand : SmtpCommand
  {
    public const string Command = "TOP";

    public int Message { get; }
    public int Lines { get; }

    /// <summary>
    /// Constructor.
    /// </summary>
    public TopCommand(int message, int lines) : base(Command)
    {
      Message = message;
      Lines = lines;
    }

    /// <summary>
    /// Execute the command.
    /// </summary>
    /// <param name="context">The execution context to operate on.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Returns true if the command executed successfully such that the transition to the next state should occurr, false
    /// if the current state is to be maintained.</returns>
    internal override async Task<bool> ExecuteAsync(SmtpSessionContext context, CancellationToken cancellationToken)
    {
      var messageStore = context.ServiceProvider.GetService<IMessageStoreFactory, IMessageStore>(context, MessageStore.Default);
      using var messageStoreContainer = new DisposableContainer<IMessageStore>(messageStore);

      if (Message < 1)
        goto noSuchMessage;
      if (Message > context.Transaction.Messages.Count)
        goto noSuchMessage;

      var message = context.Transaction.Messages[Message - 1];
      if (message == null || message.DeleteRequested)
        goto noSuchMessage;

      var bytes = await messageStoreContainer.Instance.GetAsync(context, context.Transaction.Mailbox, message, cancellationToken).ConfigureAwait(false);

      await context.Pipe.Output.WriteReplyAsync(new SmtpResponse(SmtpReplyCode.Ok, "message follows"), cancellationToken).ConfigureAwait(false);

      WriteDotStuffedWithLineLimit(context, bytes, Lines);

      context.Pipe.Output.Write(ASCII.DotArray);
      context.Pipe.Output.Write(ASCII.CrLfArray);

      await context.Pipe.Output.FlushAsync(cancellationToken).ConfigureAwait(false);

      return true;

    // -ERR no such message
    noSuchMessage:
      await context.Pipe.Output.WriteReplyAsync(new SmtpResponse(SmtpReplyCode.Err, "no such message"), cancellationToken).ConfigureAwait(false);
      return false;
    }

    /// <summary>
    /// Writes email content with dot-stuffing (RFC 1939) and enforces a line limit for the body portion.
    /// Dot-stuffing adds an extra dot at the beginning of lines that start with a dot to prevent
    /// premature message termination.
    /// </summary>
    /// <param name="context">The SMTP session context containing the output pipe.</param>
    /// <param name="data">The raw email message data to process.</param>
    /// <param name="maxLines">Maximum number of body lines to write (headers are always written in full).</param>
    /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
    /// <returns>True if the output ends with CRLF, false otherwise.</returns>
    private void WriteDotStuffedWithLineLimit(SmtpSessionContext context, byte[] data, int maxLines)
    {
      var remaining = new ReadOnlySpan<byte>(data);
      var linesWritten = 0;
      var inBody = false;

      // Process the message line by line
      var index = remaining.IndexOfNewLine();
      while (index >= 0)
      {
        // Extract current line including CRLF (index points to CR, +2 includes CRLF)
        var line = remaining.Slice(0, index + ASCII.NewLineLength);

        // Detect transition from headers to body (empty line = only newline characters)
        if (!inBody && line.Length == ASCII.NewLineLength)
        {
          inBody = true;

          // Diese Zeile MUSS IMMER geschrieben werden (RFC-konform)
          if (line.StartsWith(ASCII.DotArray))
            context.Pipe.Output.Write(ASCII.DotArray);

          context.Pipe.Output.Write(line);
        }
        else if (inBody)
        {
          // Stop writing body lines once we've reached the limit
          if (linesWritten >= maxLines)
            break;

          // Apply dot-stuffing if line starts with a dot
          if (line.StartsWith(ASCII.DotArray))
            context.Pipe.Output.Write(ASCII.DotArray);

          context.Pipe.Output.Write(line);
          linesWritten++;
        }
        else
        {
          // Write all header lines without counting toward the line limit
          if (line.StartsWith(ASCII.DotArray))
            context.Pipe.Output.Write(ASCII.DotArray);

          context.Pipe.Output.Write(line);
        }

        // Move to the next line
        remaining = remaining.Slice(index + ASCII.NewLineLength);
        index = remaining.IndexOfNewLine();
      }

      // Handle any remaining bytes (last line without CRLF or partial data)
      if (remaining.Length > 0)
      {
        // Only write remaining data if:
        // - We're still in headers, OR
        // - We're in body but haven't exceeded the line limit
        if (!inBody || linesWritten < maxLines)
        {
          // Apply dot-stuffing if remaining data starts with a dot
          if (remaining.StartsWith(ASCII.DotArray))
            context.Pipe.Output.Write(ASCII.DotArray);

          context.Pipe.Output.Write(remaining);
          context.Pipe.Output.Write(ASCII.CrLfArray);
        }
      }
    }
  }
}
