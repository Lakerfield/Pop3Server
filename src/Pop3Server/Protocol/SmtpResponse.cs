namespace Pop3Server.Protocol
{
    public class SmtpResponse
    {
        public static readonly SmtpResponse Ok = new SmtpResponse(SmtpReplyCode.Ok, "Ok");
        public static readonly SmtpResponse BeginTlsNegotiation = new SmtpResponse(SmtpReplyCode.Ok, "Begin TLS negotiation");
        //public static readonly SmtpResponse ServiceReady = new SmtpResponse(SmtpReplyCode.ServiceReady, "ready when you are");
        public static readonly SmtpResponse MailboxLocked = new SmtpResponse(SmtpReplyCode.Ok, "mailbox locked and loaded");
        public static readonly SmtpResponse UnableToLockMailbox = new SmtpResponse(SmtpReplyCode.Err, "unable to lock mailbox");
        public static readonly SmtpResponse MailboxUnavailable = new SmtpResponse(SmtpReplyCode.MailboxUnavailable, "mailbox unavailable");
        public static readonly SmtpResponse MailboxNameNotAllowed = new SmtpResponse(SmtpReplyCode.MailboxNameNotAllowed, "mailbox name not allowed");
        public static readonly SmtpResponse ServiceClosingTransmissionChannel = new SmtpResponse(SmtpReplyCode.Ok, "POP3 server saying goodbye...");
        public static readonly SmtpResponse SyntaxError = new SmtpResponse(SmtpReplyCode.SyntaxError, "syntax error");
        public static readonly SmtpResponse SizeLimitExceeded = new SmtpResponse(SmtpReplyCode.SizeLimitExceeded, "size limit exceeded");
        public static readonly SmtpResponse NoValidRecipientsGiven = new SmtpResponse(SmtpReplyCode.TransactionFailed, "no valid recipients given");
        public static readonly SmtpResponse AuthenticationFailed = new SmtpResponse(SmtpReplyCode.AuthenticationFailed, "authentication failed");
        public static readonly SmtpResponse AuthenticationSuccessful = new SmtpResponse(SmtpReplyCode.AuthenticationSuccessful, "go ahead");
        public static readonly SmtpResponse TransactionFailed = new SmtpResponse(SmtpReplyCode.TransactionFailed);
        public static readonly SmtpResponse BadSequence = new SmtpResponse(SmtpReplyCode.BadSequence, "bad sequence of commands");
        public static readonly SmtpResponse AuthenticationRequired = new SmtpResponse(SmtpReplyCode.AuthenticationRequired, "authentication required");
        public static readonly SmtpResponse QuitOk = new SmtpResponse(SmtpReplyCode.Ok, "Bye bye");
        public static readonly SmtpResponse QuitErr = new SmtpResponse(SmtpReplyCode.Err, "some deleted messages not removed");

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="replyCode">The reply code.</param>
        /// <param name="message">The reply message.</param>
        public SmtpResponse(SmtpReplyCode replyCode, string? message = null)
        {
            ReplyCode = replyCode;
            if (string.IsNullOrWhiteSpace(message))
                Lines = new string[] { };
            else
                Lines = new string[] { message };
        }
        public SmtpResponse(SmtpReplyCode replyCode, params string[] message)
        {
            ReplyCode = replyCode;
            Lines = message;
        }

        /// <summary>
        /// Gets the Reply Code.
        /// </summary>
        public SmtpReplyCode ReplyCode { get; }

        /// <summary>
        /// Gets the response message.
        /// </summary>
        public string[] Lines { get; }
    }
}
