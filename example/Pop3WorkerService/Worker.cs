using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Pop3WorkerService
{
    public sealed class Worker : BackgroundService
    {
        readonly Pop3Server.Pop3Server _pop3Server;

        public Worker(Pop3Server.Pop3Server pop3Server)
        {
          _pop3Server = pop3Server;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return _pop3Server.StartAsync(stoppingToken);
        }
    }
}