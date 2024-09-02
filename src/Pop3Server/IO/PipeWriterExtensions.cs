using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Pop3Server.Protocol;

namespace Pop3Server.IO
{
    public static class PipeWriterExtensions
    {

        internal static void WriteLines(this PipeWriter writer, IEnumerable<string> lines)
        {
            foreach (var line in lines)
                writer.WriteLine(line);
        }

        /// <summary>
        /// Write a line of text to the pipe.
        /// </summary>
        /// <param name="writer">The writer to perform the operation on.</param>
        /// <param name="text">The text to write to the writer.</param>
        internal static void WriteLine(this PipeWriter writer, string text)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            WriteLine(writer, Encoding.ASCII, text);
        }

        /// <summary>
        /// Write a line of text to the writer.
        /// </summary>
        /// <param name="writer">The writer to perform the operation on.</param>
        /// <param name="encoding">The encoding to use for the text.</param>
        /// <param name="text">The text to write to the writer.</param>
        static unsafe void WriteLine(this PipeWriter writer, Encoding encoding, string text)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            fixed (char* ptr = text)
            {
                var count = encoding.GetByteCount(ptr, text.Length);

                fixed (byte* b = writer.GetSpan(count + 2))
                {
                    encoding.GetBytes(ptr, text.Length, b, count);

                    b[count + 0] = 13;
                    b[count + 1] = 10;
                }

                writer.Advance(count + 2);
            }
        }

        /// <summary>
        /// Write a reply to the client.
        /// </summary>
        /// <param name="writer">The writer to perform the operation on.</param>
        /// <param name="response">The response to write.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task which performs the operation.</returns>
        public static ValueTask<FlushResult> WriteReplyAsync(this PipeWriter writer, SmtpResponse response, CancellationToken cancellationToken)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            var responseCode = string.Empty;
            switch (response.ReplyCode)
            {
                case SmtpReplyCode.Ok:
                    responseCode = "+OK";
                    break;
                case SmtpReplyCode.Err:
                case SmtpReplyCode.AuthenticationFailed:
                    responseCode = "-ERR";
                    break;
                default:
                    responseCode = "-TODO";
                    break;
            }
            if (response.Lines.Length == 0)
                writer.WriteLine(responseCode);
            if (response.Lines.Length >= 1)
            {
                writer.WriteLine($"{responseCode} {response.Lines[0]}");
                if (response.Lines.Length >= 2)
                    writer.WriteLines(response.Lines.Skip(1).ToArray());
            }

            return writer.FlushAsync(cancellationToken);
        }

        public static ValueTask<FlushResult> WriteReplyAsync(this PipeWriter writer, string response, CancellationToken cancellationToken)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            writer.WriteLine(response);

            return writer.FlushAsync(cancellationToken);
        }
    }
}
