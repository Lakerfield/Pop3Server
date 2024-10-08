using Pop3Server.Authentication;
using Pop3Server.Protocol;
using Pop3Server.Storage;
using System;
using Pop3Server.Net;

namespace Pop3Server.ComponentModel
{
    public sealed class ServiceProvider : IServiceProvider
    {
        public static readonly IServiceProvider Default = new ServiceProvider();

        IEndpointListenerFactory _endpointListenerFactory;
        IUserAuthenticatorFactory _userAuthenticatorFactory;
        ISmtpCommandFactory _smtpCommandFactory;
        IMessageStoreFactory _messageStoreFactory;
        
        public ServiceProvider()
        {
            Add(UserAuthenticator.Default);
            Add(MessageStore.Default);
        }

        /// <summary>
        /// Add an instance of the endpoint listener factory.
        /// </summary>
        /// <param name="endpointListenerFactory">The endpoint listener factory.</param>
        public void Add(IEndpointListenerFactory endpointListenerFactory)
        {
            _endpointListenerFactory = endpointListenerFactory;
        }

        /// <summary>
        /// Add an instance of the user authenticator factory.
        /// </summary>
        /// <param name="userAuthenticatorFactory">The user authenticator factory.</param>
        public void Add(IUserAuthenticatorFactory userAuthenticatorFactory)
        {
            _userAuthenticatorFactory = userAuthenticatorFactory;
        }

        /// <summary>
        /// Add an instance of the user authenticator.
        /// </summary>
        /// <param name="userAuthenticator">The user authenticator.</param>
        public void Add(IUserAuthenticator userAuthenticator)
        {
            _userAuthenticatorFactory = new DelegatingUserAuthenticatorFactory(context => userAuthenticator);
        }

        /// <summary>
        /// Add an instance of the SMTP command factory.
        /// </summary>
        /// <param name="smtpCommandFactory">The SMTP command factory.</param>
        public void Add(ISmtpCommandFactory smtpCommandFactory)
        {
            _smtpCommandFactory = smtpCommandFactory;
        }

        /// <summary>
        /// Add an instance of the Message Store Factory.
        /// </summary>
        /// <param name="messageStoreFactory">The message store factory.</param>
        public void Add(IMessageStoreFactory messageStoreFactory)
        {
            _messageStoreFactory = messageStoreFactory;
        }

        /// <summary>
        /// Add an instance of the Message Store.
        /// </summary>
        /// <param name="messageStore">The message store.</param>
        public void Add(IMessageStore messageStore)
        {
            _messageStoreFactory = new DelegatingMessageStoreFactory(context => messageStore);
        }

        /// <summary>
        /// Gets the service object of the specified type.
        /// </summary>
        /// <param name="serviceType">An object that specifies the type of service object to get.</param>
        /// <returns>A service object of type <paramref name="serviceType">serviceType</paramref>.   -or-  null if there is no service object of type <paramref name="serviceType">serviceType</paramref>.</returns>
        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(IEndpointListenerFactory))
            {
                return _endpointListenerFactory;
            }

            if (serviceType == typeof(IUserAuthenticatorFactory))
            {
                return _userAuthenticatorFactory;
            }

            if (serviceType == typeof(ISmtpCommandFactory))
            {
                return _smtpCommandFactory;
            }

            if (serviceType == typeof(IMessageStoreFactory))
            {
                return _messageStoreFactory;
            }

            throw new NotSupportedException(serviceType.ToString());
        }
    }
}
