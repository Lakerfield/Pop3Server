using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SmtpServer.IO;

namespace SmtpServer.Protocol
{
    public sealed class RetrCommand : SmtpCommand
    {
      public const string Command = "RETR";

      public int Message { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public RetrCommand(int message) : base(Command)
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
          var bytes = File.ReadAllBytes(@"E:\xx-end.edi");


            await context.Pipe.Output.WriteReplyAsync(new SmtpResponse(SmtpReplyCode.Ok, @$"{bytes.Length} octets
{System.Text.Encoding.ASCII.GetString(bytes)}
."), cancellationToken).ConfigureAwait(false);

            return true;
        }
    }
}
