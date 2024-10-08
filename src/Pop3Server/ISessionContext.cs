using System;
using System.Collections.Generic;
using Pop3Server.IO;

namespace Pop3Server
{
    public interface ISessionContext
    {
        /// <summary>
        /// Fired when a command is about to execute.
        /// </summary>
        event EventHandler<SmtpCommandEventArgs> CommandExecuting;

        /// <summary>
        /// Fired when a command has finished executing.
        /// </summary>
        event EventHandler<SmtpCommandEventArgs> CommandExecuted;

        /// <summary>
        /// Fired when a response exception has occured. 
        /// </summary>
        event EventHandler<SmtpResponseExceptionEventArgs> ResponseException;

        /// <summary>
        /// Fired when the session has been authenticated.
        /// </summary>
        event EventHandler<EventArgs> SessionAuthenticated;

        /// <summary>
        /// The service provider instance. 
        /// </summary>
        IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Gets the options that the server was created with.
        /// </summary>
        IPop3ServerOptions ServerOptions { get; }

        /// <summary>
        /// Gets the endpoint definition.
        /// </summary>
        IEndpointDefinition EndpointDefinition { get; }

        /// <summary>
        /// Gets the pipeline to read from and write to.
        /// </summary>
        ISecurableDuplexPipe Pipe { get; }

        /// <summary>
        /// Returns the authentication context.
        /// </summary>
        AuthenticationContext Authentication { get; }

        /// <summary>
        /// Returns a set of propeties for the current session.
        /// </summary>
        IDictionary<string, object> Properties { get; }
    }
}
