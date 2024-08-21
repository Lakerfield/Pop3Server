using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using Pop3Server.ComponentModel;
using Pop3Server.IO;
using Pop3Server.Storage;

namespace Pop3Server.Protocol
{
    public sealed class DeleCommand : SmtpCommand
    {
      public const string Command = "DELE";

      public int Message { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public DeleCommand(int message) : base(Command)
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
            if (message == null)
                goto noSuchMessage;

            if (message.DeleteRequested == false)
            {
                message.DeleteRequested = true;
                await context.Pipe.Output.WriteReplyAsync(new SmtpResponse(SmtpReplyCode.Ok, @$"message {Message} deleted"), cancellationToken).ConfigureAwait(false);
            }
            else
            {
                await context.Pipe.Output.WriteReplyAsync(new SmtpResponse(SmtpReplyCode.Err, @$"message {Message} already deleted"), cancellationToken).ConfigureAwait(false);
            }

            return true;

            noSuchMessage:
            await context.Pipe.Output.WriteReplyAsync(new SmtpResponse(SmtpReplyCode.Err, "no such message"), cancellationToken).ConfigureAwait(false);
            return false;
        }
    }
}
