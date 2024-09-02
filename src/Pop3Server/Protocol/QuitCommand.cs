using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
using Pop3Server.IO;
using System.IO;
using System.Linq;
using Pop3Server.ComponentModel;
using Pop3Server.Storage;

namespace Pop3Server.Protocol
{
    public sealed class QuitCommand : SmtpCommand
    {
        public const string Command = "QUIT";

        /// <summary>
        /// Constructor.
        /// </summary>
        public QuitCommand() : base(Command) { }

        /// <summary>
        /// Execute the command.
        /// </summary>
        /// <param name="context">The execution context to operate on.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Returns true if the command executed successfully such that the transition to the next state should occurr, false 
        /// if the current state is to be maintained.</returns>
        internal override async Task<bool> ExecuteAsync(SmtpSessionContext context, CancellationToken cancellationToken)
        {
            context.IsQuitRequested = true;

            try
            {
                var messageStore = context.ServiceProvider.GetService<IMessageStoreFactory, IMessageStore>(context, MessageStore.Default);
                using var messageStoreContainer = new DisposableContainer<IMessageStore>(messageStore);

                var response = SmtpResponse.QuitOk;
                try
                {
                    if (context.Transaction.Mailbox != null)
                    {
                        try
                        {
                            var deletedMessages = context.Transaction.Messages.Where(m => m.DeleteRequested).ToArray();
                            if (deletedMessages.Length > 0)
                            {
                                await messageStoreContainer.Instance
                                    .DeleteAsync(context, context.Transaction.Mailbox, deletedMessages, cancellationToken)
                                    .ConfigureAwait(false);
                            }
                        }
                        catch
                        {
                            response = SmtpResponse.QuitErr;
                            throw;
                        }
                    }
                }
                finally
                {
                    if (context.Transaction.HasLockedMailbox && context.Transaction.Mailbox != null)
                    {
                        await messageStoreContainer.Instance.UnlockMailboxAsync(context, context.Transaction.Mailbox, cancellationToken);
                        context.Transaction.HasLockedMailbox = false;
                    }

                    await context.Pipe.Output.WriteReplyAsync(response, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (IOException ioException)
            {
                if (ioException.GetBaseException() is SocketException socketException)
                {
                    // Some mail servers will send the QUIT command and then disconnect before
                    // waiting for the 221 response from the server. This doesnt follow the spec but
                    // we can gracefully handle this situation as in theory everything should be fine
                    if (socketException.SocketErrorCode == SocketError.ConnectionReset)
                    {
                        return true;
                    }
                }

                throw ioException;
            }

            return true;
        }
    }
}
