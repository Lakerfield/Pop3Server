using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Pop3Server.ComponentModel;
using Pop3Server.IO;
using Pop3Server.Storage;

namespace Pop3Server.Protocol
{
    public sealed class RetrCommand : SmtpCommand
    {
      public const string Command = "RETR";

      public int Message { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public RetrCommand(int message) : base(Command)
        {
          Message = message;
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

            //+OK message follows

            var bytes = await messageStoreContainer.Instance.GetAsync(context, message, cancellationToken).ConfigureAwait(false);

            await context.Pipe.Output.WriteReplyAsync(new SmtpResponse(SmtpReplyCode.Ok, @$"message follows {bytes.Length} octets"),cancellationToken).ConfigureAwait(false);

            context.Pipe.Output.Write(bytes);

            var ending = new byte[] { 13, 10, 46, 13, 10 };

            context.Pipe.Output.Write(ending);

            await context.Pipe.Output.FlushAsync(cancellationToken).ConfigureAwait(false);

//             await context.Pipe.Output.WriteReplyAsync(new SmtpResponse(SmtpReplyCode.Ok, @$"{bytes.Length} octets
// {System.Text.Encoding.ASCII.GetString(bytes)}
// ."), cancellationToken).ConfigureAwait(false);

            return true;

            // -ERR no such message
            noSuchMessage:
            await context.Pipe.Output.WriteReplyAsync(new SmtpResponse(SmtpReplyCode.Err, "no such message"), cancellationToken).ConfigureAwait(false);
            return false;
        }
    }
}
