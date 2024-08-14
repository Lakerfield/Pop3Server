using System.Threading;
using System.Threading.Tasks;
using Pop3Server.Authentication;
using Pop3Server.ComponentModel;
using Pop3Server.IO;
using Pop3Server.Storage;

namespace Pop3Server.Protocol
{
    public sealed class PassCommand : SmtpCommand
    {
      public const string Command = "PASS";

      public string Password { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public PassCommand(string password) : base(Command)
        {
          Password = password;
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
            var user = context.Authentication.User ?? "";

            var userAuthenticator = context.ServiceProvider.GetService<IUserAuthenticatorFactory, IUserAuthenticator>(context, UserAuthenticator.Default);

            using (var container = new DisposableContainer<IUserAuthenticator>(userAuthenticator))
            {
                if (await container.Instance.AuthenticateAsync(context, user, Password, cancellationToken).ConfigureAwait(false) == false)
                {
                    var remaining = context.ServerOptions.MaxAuthenticationAttempts - ++context.AuthenticationAttempts;
                    var response = new SmtpResponse(SmtpReplyCode.AuthenticationFailed, $"authentication failed, {remaining} attempt(s) remaining.");

                    await context.Pipe.Output.WriteReplyAsync(response, cancellationToken).ConfigureAwait(false);

                    if (remaining <= 0)
                    {
                        throw new SmtpResponseException(SmtpResponse.ServiceClosingTransmissionChannel, true);
                    }

                    return false;
                }
            }

            await context.Pipe.Output.WriteReplyAsync(SmtpResponse.Ok, cancellationToken).ConfigureAwait(false);

            context.Authentication = new AuthenticationContext(true, user);
            context.RaiseSessionAuthenticated();


            // TODO Lock

            var messageStore = context.ServiceProvider.GetService<IMessageStoreFactory, IMessageStore>(context, MessageStore.Default);
            using var messageStoreContainer = new DisposableContainer<IMessageStore>(messageStore);
            var messages = await messageStoreContainer.Instance.GetMessagesAsync(context, context.Transaction.Mailbox, cancellationToken).ConfigureAwait(false);

            foreach (var message in messages)
                context.Transaction.Messages.Add(message);

            return true;
        }
    }
}
