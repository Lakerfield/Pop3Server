using System;
using System.Threading;
using System.Threading.Tasks;
using Pop3Server.IO;

namespace Pop3Server.Net
{
    public interface IEndpointListener : IDisposable
    {
        /// <summary>
        /// Returns a securtable pipe to the endpoint.
        /// </summary>
        /// <param name="context">The session context that the pipe is being created for.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The securable pipe from the endpoint.</returns>
        Task<ISecurableDuplexPipe> GetPipeAsync(ISessionContext context, CancellationToken cancellationToken);
    }
}
