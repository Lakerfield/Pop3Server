using System.Threading;
using System.Threading.Tasks;
using Pop3Server.IO;

namespace Pop3Server.Protocol
{
    public sealed class TopCommand : SmtpCommand
    {
      public const string Command = "TOP";

      public int Message { get; }
      public int Lines { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public TopCommand(int message, int lines) : base(Command)
        {
          Message = message;
          Lines = lines;
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
            await context.Pipe.Output.WriteReplyAsync(SmtpResponse.Ok, cancellationToken).ConfigureAwait(false);

            return true;
        }
    }
}
