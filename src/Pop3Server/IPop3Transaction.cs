using System.Collections.Generic;
using Pop3Server.Mail;

namespace Pop3Server
{
    public interface IPop3Transaction
    {
        /// <summary>
        /// Gets or sets the current mailbox.
        /// </summary>
        IMailbox Mailbox { get; set; }

        /// <summary>
        /// Gets the collection of messages in the mailboxes.
        /// </summary>
        IList<IPop3Message> Messages { get; }

        /// <summary>
        /// The list of parameters that were supplied by the client.
        /// </summary>
        IReadOnlyDictionary<string, string> Parameters { get; }
    }

    public interface IPop3Message
    {
        string Id { get; set; }
        int Size { get; set; }
        bool DeleteRequested { get; set; }
    }
}
