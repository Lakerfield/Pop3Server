using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Pop3Server.IO;
using Pop3Server.StateMachine;

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
            await context.Pipe.Output.WriteReplyAsync(new SmtpResponse(SmtpReplyCode.Ok, GetCapabilities().ToArray()), cancellationToken).ConfigureAwait(false);

            return true;

            IEnumerable<string> GetCapabilities()
            {
                yield return "Capability list follows";

                if (context.Transaction.CapaState == SmtpStateId.Authorization)
                {
                    //yield return "AUTH";
                    yield return "USER";
                    yield return "PASS";
                }

                if (context.Transaction.CapaState == SmtpStateId.Authorization)
                    yield return "STLS";

                if (context.Transaction.CapaState == SmtpStateId.AuthorizationWaitForPassword)
                    yield return "PASS";

                yield return "TOP";

                if (context.Transaction.CapaState == SmtpStateId.Transaction)
                {
                    yield return "UIDL";
                    yield return "TOP";
                    yield return "RSET";
                }

                yield return ".";
            }
        }
    }
}
