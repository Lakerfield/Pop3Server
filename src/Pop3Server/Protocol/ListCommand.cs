using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Pop3Server.IO;

namespace Pop3Server.Protocol
{
    public sealed class ListCommand : SmtpCommand
    {
      public const string Command = "LIST";

      public int Message { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ListCommand(int message) : base(Command)
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
            if (Message < 0 || Message > context.Transaction.Messages.Count)
            {
                await context.Pipe.Output.WriteReplyAsync(new SmtpResponse(SmtpReplyCode.Err, $"no such message, only {context.Transaction.Messages.Count} messages in maildrop"), cancellationToken).ConfigureAwait(false);
                return true;
            }

            var response = new SmtpResponse(SmtpReplyCode.Ok, GetResponse().ToArray());

            await context.Pipe.Output.WriteReplyAsync(response, cancellationToken).ConfigureAwait(false);

            return true;

            IEnumerable<string> GetResponse()
            {
                if (Message > 0)
                {
                    yield return $"{Message} {context.Transaction.Messages[Message-1]}";
                    yield break;
                }

                var count = context.Transaction.Messages.Count;
                var totalSize = context.Transaction.Messages.Sum(m => m.Size);
                yield return $"{count} messages ({totalSize} octets)";

                var messages = context.Transaction.Messages;
                for (var i = 0; i < messages.Count; i++)
                    yield return $"{i+1} {messages[i].Size}";
                yield return ".";
            }
        }
    }
}
