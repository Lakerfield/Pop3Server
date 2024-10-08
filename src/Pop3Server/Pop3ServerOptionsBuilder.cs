using System;
using System.Collections.Generic;

namespace Pop3Server
{
    public sealed class Pop3ServerOptionsBuilder
    {
        readonly List<Action<Pop3ServerOptions>> _setters = new List<Action<Pop3ServerOptions>>();

        /// <summary>
        /// Builds the options that have been set and returns the built instance.
        /// </summary>
        /// <returns>The server options that have been set.</returns>
        public IPop3ServerOptions Build()
        {
            var serverOptions = new Pop3ServerOptions
            {
                Endpoints = new List<IEndpointDefinition>(),
                MaxRetryCount = 5,
                MaxAuthenticationAttempts = 3,
                NetworkBufferSize = 128,
                CommandWaitTimeout = TimeSpan.FromMinutes(5)
            };

            _setters.ForEach(setter => setter(serverOptions));

            return serverOptions;
        }

        /// <summary>
        /// Sets the server name.
        /// </summary>
        /// <param name="value">The name of the server.</param>
        /// <returns>A OptionsBuilder to continue building on.</returns>
        public Pop3ServerOptionsBuilder ServerName(string value)
        {
            _setters.Add(options => options.ServerName = value);

            return this;
        }

        /// <summary>
        /// Adds a definition for an endpoint to listen on.
        /// </summary>
        /// <param name="value">The endpoint to listen on.</param>
        /// <returns>A OptionsBuilder to continue building on.</returns>
        public Pop3ServerOptionsBuilder Endpoint(IEndpointDefinition value)
        {
            _setters.Add(options => options.Endpoints.Add(value));

            return this;
        }

        /// <summary>
        /// Adds a definition for an endpoint to listen on.
        /// </summary>
        /// <param name="configure">The endpoint to listen on.</param>
        /// <returns>A OptionsBuilder to continue building on.</returns>
        public Pop3ServerOptionsBuilder Endpoint(Action<EndpointDefinitionBuilder> configure)
        {
            var endpointDefinitionBuilder = new EndpointDefinitionBuilder();
            configure(endpointDefinitionBuilder);

            return Endpoint(endpointDefinitionBuilder.Build());
        }

        /// <summary>
        /// Sets the maximum message size.
        /// </summary>
        /// <param name="value">The maximum message size to allow.</param>
        /// <returns>A OptionsBuilder to continue building on.</returns>
        public Pop3ServerOptionsBuilder MaxMessageSize(int value)
        {
            _setters.Add(options => options.MaxMessageSize = value);

            return this;
        }

        /// <summary>
        /// Sets the maximum number of retries for a failed command.
        /// </summary>
        /// <param name="value">The maximum number of retries allowed for a failed command.</param>
        /// <returns>A OptionsBuilder to continue building on.</returns>
        public Pop3ServerOptionsBuilder MaxRetryCount(int value)
        {
            _setters.Add(options => options.MaxRetryCount = value);

            return this;
        }

        /// <summary>
        /// Sets the maximum number of authentication attempts.
        /// </summary>
        /// <param name="value">The maximum number of authentication attempts for a failed authentication.</param>
        /// <returns>A OptionsBuilder to continue building on.</returns>
        public Pop3ServerOptionsBuilder MaxAuthenticationAttempts(int value)
        {
            _setters.Add(options => options.MaxAuthenticationAttempts = value);

            return this;
        }

        /// <summary>
        /// Sets the size of the buffer for each read operation.
        /// </summary>
        /// <param name="value">The buffer size for each read operation.</param>
        /// <returns>An OptionsBuilder to continue building on.</returns>
        public Pop3ServerOptionsBuilder NetworkBufferSize(int value)
        {
            _setters.Add(options => options.NetworkBufferSize = value);

            return this;
        }

        /// <summary>
        /// Sets the timeout used when waiting for a command from the client.
        /// </summary>
        /// <param name="value">The timeout used when waiting for a command from the client.</param>
        /// <returns>An OptionsBuilder to continue building on.</returns>
        public Pop3ServerOptionsBuilder CommandWaitTimeout(TimeSpan value)
        {
            _setters.Add(options => options.CommandWaitTimeout = value);
            
            return this;
        }

        #region Pop3ServerOptions

        class Pop3ServerOptions : IPop3ServerOptions
        {
            /// <summary>
            /// Gets or sets the maximum size of a message.
            /// </summary>
            public int MaxMessageSize { get; set; }

            /// <summary>
            /// The maximum number of retries before quitting the session.
            /// </summary>
            public int MaxRetryCount { get; set; }

            /// <summary>
            /// The maximum number of authentication attempts.
            /// </summary>
            public int MaxAuthenticationAttempts { get; set; }

            /// <summary>
            /// Gets or sets the SMTP server name.
            /// </summary>
            public string ServerName { get; set; }

            /// <summary>
            /// Gets or sets the endpoint to listen on.
            /// </summary>
            internal List<IEndpointDefinition> Endpoints { get; set; }

            /// <summary>
            /// Gets or sets the endpoint to listen on.
            /// </summary>
            IReadOnlyList<IEndpointDefinition> IPop3ServerOptions.Endpoints => Endpoints;

            /// <summary>
            /// The timeout to use when waiting for a command from the client.
            /// </summary>
            public TimeSpan CommandWaitTimeout { get; set; }

            /// <summary>
            /// The size of the buffer that is read from each call to the underlying network client.
            /// </summary>
            public int NetworkBufferSize { get; set; }
        }

        #endregion
    }
}
