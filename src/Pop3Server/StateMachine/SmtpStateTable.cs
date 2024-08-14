using System;
using System.Collections;
using System.Collections.Generic;
using Pop3Server.Protocol;

namespace Pop3Server.StateMachine
{
    internal sealed class SmtpStateTable : IEnumerable
    {
        internal static readonly SmtpStateTable Shared = new SmtpStateTable
        {
            new SmtpState(SmtpStateId.Authorization)
            {
                { QuitCommand.Command },
                { ProxyCommand.Command },
                { CapaCommand.Command },
                { StlsCommand.Command, CanAcceptStls, SmtpStateId.AuthorizationSecure },
                { UserCommand.Command, context => context.EndpointDefinition.AllowUnsecureAuthentication && context.Authentication.IsAuthenticated == false },
                { PassCommand.Command, context => context.EndpointDefinition.AllowUnsecureAuthentication && context.Authentication.IsAuthenticated == false },
                { AuthCommand.Command, context => context.EndpointDefinition.AllowUnsecureAuthentication && context.Authentication.IsAuthenticated == false },
            },
            new SmtpState(SmtpStateId.AuthorizationSecure)
            {
                { QuitCommand.Command },
                { CapaCommand.Command },
                { UserCommand.Command, context => context.Authentication.IsAuthenticated == false },
                { PassCommand.Command },//, SmtpStateId.Transaction },
                { AuthCommand.Command, context => context.Authentication.IsAuthenticated == false },
            //},
            //new SmtpState(SmtpStateId.Transaction)
            //{
            //    { NoopCommand.Command },
            //    { RsetCommand.Command },
            //    { QuitCommand.Command },
            //    { CapaCommand.Command },
                { StatCommand.Command },
                { ListCommand.Command },
                { RetrCommand.Command },
                { DeleCommand.Command },
                { TopCommand.Command },
                { UidlCommand.Command },
            },
            new SmtpState(SmtpStateId.Update)
            {
                { NoopCommand.Command },
                { QuitCommand.Command },
            },
        };

        static bool CanAcceptStls(SmtpSessionContext context)
        {
            return context.EndpointDefinition.CertificateFactory != null && context.Pipe.IsSecure == false;
        }

        readonly IDictionary<SmtpStateId, SmtpState> _states = new Dictionary<SmtpStateId, SmtpState>();

        internal SmtpState this[SmtpStateId stateId] => _states[stateId];

        /// <summary>
        /// Add the state to the table.
        /// </summary>
        /// <param name="state"></param>
        void Add(SmtpState state)
        {
            _states.Add(state.StateId, state);
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator"></see> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            // this is just here for the collection initializer syntax to work
            throw new NotImplementedException();
        }
    }
}
