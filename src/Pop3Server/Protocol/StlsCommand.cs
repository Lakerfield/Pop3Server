using System;
using System.Threading;
using System.Threading.Tasks;
using Pop3Server.IO;

namespace Pop3Server.Protocol
{
    public sealed class StlsCommand : SmtpCommand
    {
        public const string Command = "STLS";

        /// <summary>
        /// Constructor.
        /// </summary>
        public StlsCommand() : base(Command) { }

        /// <summary>
        /// Execute the command.
        /// </summary>
        /// <param name="context">The execution context to operate on.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Returns true if the command executed successfully such that the transition to the next state should occurr, false 
        /// if the current state is to be maintained.</returns>
        internal override async Task<bool> ExecuteAsync(SmtpSessionContext context, CancellationToken cancellationToken)
        {
            if (context.Pipe.IsSecure)
            {
                await context.Pipe.Output.WriteReplyAsync(SmtpResponse.TlsAlreadyActive, cancellationToken).ConfigureAwait(false);
                return false;
            }

            var certificateFactory = context.EndpointDefinition.CertificateFactory;
            if (certificateFactory == null)
            {
                await context.Pipe.Output.WriteReplyAsync(SmtpResponse.TlsNotAvailable, cancellationToken).ConfigureAwait(false);
                return false;
            }

            var certificate = certificateFactory.GetServerCertificate(context);
            if (certificate == null)
            {
                await context.Pipe.Output.WriteReplyAsync(SmtpResponse.TlsNotAvailable, cancellationToken).ConfigureAwait(false);
                return false;
            }

            await context.Pipe.Output.WriteReplyAsync(SmtpResponse.BeginTlsNegotiation, cancellationToken).ConfigureAwait(false);

            var protocols = context.EndpointDefinition.SupportedSslProtocols;

            await context.Pipe.UpgradeAsync(certificate, protocols, cancellationToken).ConfigureAwait(false);

            return true;
        }
    }
}
