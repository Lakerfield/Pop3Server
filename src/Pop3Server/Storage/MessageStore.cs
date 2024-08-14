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

        /// <summary>
        /// Get the given message to the underlying storage system.
        /// </summary>
        /// <param name="context">The session context.</param>
        /// <param name="message">The POP3 message to retrieve.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The buffer that contains the message content.</returns>
        public abstract Task<byte[]> GetAsync(ISessionContext context, IPop3Message message, CancellationToken cancellationToken);

        public abstract Task<IPop3Message[]> GetMessagesAsync(ISessionContext context, IMailbox mailbox, CancellationToken cancellationToken);

        sealed class DefaultMessageStore : MessageStore
        {
            /// <summary>
            /// Get the given message to the underlying storage system.
            /// </summary>
            /// <param name="context">The session context.</param>
            /// <param name="message">The POP3 message to retrieve.</param>
            /// <param name="cancellationToken">The cancellation token.</param>
            /// <returns>The buffer that contains the message content.</returns>
            public override async Task<byte[]> GetAsync(ISessionContext context, IPop3Message message, CancellationToken cancellationToken)
            {
                return Array.Empty<byte>();
            }

            public override async Task<IPop3Message[]> GetMessagesAsync(ISessionContext context, IMailbox mailbox, CancellationToken cancellationToken)
            {
                return Array.Empty<IPop3Message>();
            }
        }
    }
}
