using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Pop3Server
{
  using CurrentService = Pop3ServerBackgroundService;

  public class Pop3ServerBackgroundService : BackgroundService
  {
    private readonly IPop3ServerOptions _options;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CurrentService> _logger;

    public Pop3ServerBackgroundService(
      IPop3ServerOptions options,
      IServiceProvider serviceProvider,
      ILogger<CurrentService> logger)
    {
      _options = options;
      _serviceProvider = serviceProvider;
      _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      _logger.LogDebug($"{nameof(CurrentService)} is starting.");

      var pop3Server = new Pop3Server(_options, _serviceProvider);

      _logger.LogDebug($"{nameof(CurrentService)} is started.");

      stoppingToken.Register(() =>
        _logger.LogDebug($"{nameof(CurrentService)} is stopping."));

      await pop3Server.StartAsync(stoppingToken);

      _logger.LogDebug($"{nameof(CurrentService)} is stopped.");
    }
  }
  
}
