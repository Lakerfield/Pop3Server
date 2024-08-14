using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using Pop3Server.Mail;
using Pop3Server.Protocol;

namespace Pop3Server.Storage
{
    public interface IMessageStore
    {
        /// <summary>
        /// Get the given message to the underlying storage system.
        /// </summary>
        /// <param name="context">The session context.</param>
        /// <param name="message">The POP3 message to retrieve.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The buffer that contains the message content.</returns>
        Task<byte[]> GetAsync(ISessionContext context, IPop3Message message, CancellationToken cancellationToken);

        Task<IPop3Message[]> GetMessagesAsync(ISessionContext context, IMailbox mailbox, CancellationToken cancellationToken);

    }
}
