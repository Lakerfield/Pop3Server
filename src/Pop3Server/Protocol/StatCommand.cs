using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Pop3Server.IO;

namespace Pop3Server.Protocol
{
    public sealed class StatCommand : SmtpCommand
    {
        public const string Command = "STAT";

        /// <summary>
        /// Constructor.
        /// </summary>
        public StatCommand() : base(Command) { }

        /// <summary>
        /// Execute the command.
        /// </summary>
        /// <param name="context">The execution context to operate on.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Returns true if the command executed successfully such that the transition to the next state should occurr, false 
        /// if the current state is to be maintained.</returns>
        internal override async Task<bool> ExecuteAsync(SmtpSessionContext context, CancellationToken cancellationToken)
        {
            var count = context.Transaction.Messages.Count;
            var totalSize = context.Transaction.Messages.Sum(m => m.Size);

            await context.Pipe.Output.WriteReplyAsync(new SmtpResponse(SmtpReplyCode.Ok, $"{count} {totalSize}"), cancellationToken).ConfigureAwait(false);

            return true;
        }
    }
}
