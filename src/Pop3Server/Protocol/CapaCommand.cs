using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Pop3Server.IO;

namespace Pop3Server.Protocol
{
    public sealed class CapaCommand : SmtpCommand
    {
        public const string Command = "CAPA";

        /// <summary>
        /// Constructor.
        /// </summary>
        public CapaCommand() : base(Command) { }

        /// <summary>
        /// Execute the command.
        /// </summary>
        /// <param name="context">The execution context to operate on.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Returns true if the command executed successfully such that the transition to the next state should occurr, false
        /// if the current state is to be maintained.</returns>
        internal override async Task<bool> ExecuteAsync(SmtpSessionContext context, CancellationToken cancellationToken)
        {
//             await context.Pipe.Output.WriteReplyAsync(new SmtpResponse(SmtpReplyCode.Ok, @"Capability list follows
// USER
// UIDL
// TOP
// STLS
// ."), cancellationToken).ConfigureAwait(false);
//TODO: rest
            await context.Pipe.Output.WriteReplyAsync(new SmtpResponse(SmtpReplyCode.Ok, GetCapabilities().ToArray()), cancellationToken).ConfigureAwait(false);

            return true;

            IEnumerable<string> GetCapabilities()
            {
                yield return "Capability list follows";
                yield return "USER";
                yield return "TOP";
                yield return ".";
            }
        }
    }
}
