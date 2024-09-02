using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pop3Server
{
    internal sealed class Pop3SessionManager
    {
        readonly Pop3Server _pop3Server;
        readonly HashSet<Pop3SessionHandle> _sessions = new HashSet<Pop3SessionHandle>();
        readonly object _sessionsLock = new object();

        internal Pop3SessionManager(Pop3Server pop3Server)
        {
            _pop3Server = pop3Server;
        }

        internal void Run(SmtpSessionContext sessionContext, CancellationToken cancellationToken)
        {
            var handle = new Pop3SessionHandle(new SmtpSession(sessionContext), sessionContext);
            Add(handle);

            handle.CompletionTask = RunAsync(handle, cancellationToken);

            // ReSharper disable once MethodSupportsCancellation
            handle.CompletionTask.ContinueWith(
                task =>
                {
                    Remove(handle);
                });
        }

        async Task RunAsync(Pop3SessionHandle handle, CancellationToken cancellationToken)
        {
            try
            {
                await UpgradeAsync(handle, cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();

                _pop3Server.OnSessionCreated(new SessionEventArgs(handle.SessionContext));

                await handle.Session.RunAsync(cancellationToken);

                _pop3Server.OnSessionCompleted(new SessionEventArgs(handle.SessionContext));
            }
            catch (OperationCanceledException)
            {
                _pop3Server.OnSessionCancelled(new SessionEventArgs(handle.SessionContext));
            }
            catch (Exception ex)
            {
                _pop3Server.OnSessionFaulted(new SessionFaultedEventArgs(handle.SessionContext, ex));
            }
            finally
            {
                await handle.SessionContext.Pipe.Input.CompleteAsync();

                handle.SessionContext.Pipe.Dispose();
            }
        }

        async Task UpgradeAsync(Pop3SessionHandle handle, CancellationToken cancellationToken)
        {
            var endpoint = handle.SessionContext.EndpointDefinition;

            if (endpoint.IsSecure && endpoint.CertificateFactory != null)
            {
                var serverCertificate = endpoint.CertificateFactory.GetServerCertificate(handle.SessionContext);

                await handle.SessionContext.Pipe.UpgradeAsync(serverCertificate, endpoint.SupportedSslProtocols, cancellationToken).ConfigureAwait(false);
            }
        }

        internal Task WaitAsync()
        {
            IReadOnlyList<Task> tasks;

            lock (_sessionsLock)
            {
                tasks = _sessions.Select(session => session.CompletionTask).ToList();
            }

            return Task.WhenAll(tasks);
        }

        void Add(Pop3SessionHandle handle)
        {
            lock (_sessionsLock)
            {
                _sessions.Add(handle);
            }
        }

        void Remove(Pop3SessionHandle handle)
        {
            lock (_sessionsLock)
            {
                _sessions.Remove(handle);
            }
        }

        class Pop3SessionHandle
        {
            public Pop3SessionHandle(SmtpSession session, SmtpSessionContext sessionContext)
            {
                Session = session;
                SessionContext = sessionContext;
            }

            public SmtpSession Session { get; }

            public SmtpSessionContext SessionContext { get; }

            public Task CompletionTask { get; set; }
        }
    }
}
