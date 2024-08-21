using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using Pop3Server.Mail;
using Pop3Server.Protocol;

namespace Pop3Server.Storage
{
    public abstract class MessageStore : IMessageStore
    {
        public static readonly IMessageStore Default = new DefaultMessageStore();

        public abstract Task<bool> LockMailboxAsync(ISessionContext context, IMailbox mailbox, CancellationToken cancellationToken);

        public abstract Task UnlockMailboxAsync(ISessionContext context, IMailbox mailbox, CancellationToken cancellationToken);

        /// <summary>
        /// Get the given message to the underlying storage system.
        /// </summary>
        /// <param name="context">The session context.</param>
        /// <param name="message">The POP3 message to retrieve.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The buffer that contains the message content.</returns>
        public abstract Task<byte[]> GetAsync(ISessionContext context, IMailbox mailbox, IPop3Message message, CancellationToken cancellationToken);

        public abstract Task<IPop3Message[]> GetMessagesAsync(ISessionContext context, IMailbox mailbox, CancellationToken cancellationToken);

        public abstract Task DeleteAsync(ISessionContext context, IMailbox mailbox, IPop3Message[] messages, CancellationToken cancellationToken);

        sealed class DefaultMessageStore : MessageStore
        {
            public override Task<bool> LockMailboxAsync(ISessionContext context, IMailbox mailbox, CancellationToken cancellationToken)
            {
                return Task.FromResult(true);
            }

            public override Task UnlockMailboxAsync(ISessionContext context, IMailbox mailbox, CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }

            /// <summary>
            /// Get the given message to the underlying storage system.
            /// </summary>
            /// <param name="context">The session context.</param>
            /// <param name="message">The POP3 message to retrieve.</param>
            /// <param name="cancellationToken">The cancellation token.</param>
            /// <returns>The buffer that contains the message content.</returns>
            public override async Task<byte[]> GetAsync(ISessionContext context, IMailbox mailbox, IPop3Message message, CancellationToken cancellationToken)
            {
                return Array.Empty<byte>();
            }

            public override async Task<IPop3Message[]> GetMessagesAsync(ISessionContext context, IMailbox mailbox, CancellationToken cancellationToken)
            {
                return Array.Empty<IPop3Message>();
            }

            public override Task DeleteAsync(ISessionContext context, IMailbox mailbox, IPop3Message[] messages, CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }
        }
    }
}
