using System.Collections.Generic;
using System.Collections.ObjectModel;
using Pop3Server.Mail;

namespace Pop3Server
{
    internal sealed class Pop3Transaction : IPop3Transaction
    {
        /// <summary>
        /// Reset the current transaction.
        /// </summary>
        public void Reset()
        {
            Mailbox = null;
            Messages = new Collection<IPop3Message>();
            Parameters = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());
        }

        /// <summary>
        /// Gets or sets the mailbox that is sending the message.
        /// </summary>
        public IMailbox Mailbox { get; set; }

        /// <summary>
        /// Gets or sets the collection of mailboxes that the message is to be delivered to.
        /// </summary>
        public IList<IPop3Message> Messages { get; set; } = new Collection<IPop3Message>();

        /// <summary>
        /// The list of parameters that were supplied by the client.
        /// </summary>
        public IReadOnlyDictionary<string, string> Parameters { get; set; } = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());
    }
}
