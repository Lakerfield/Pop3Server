using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Pop3Server
{
  public class Pop3ServerBackgroundService : BackgroundService
  {
    private readonly IPop3ServerOptions _options;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<Pop3ServerBackgroundService> _logger;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="options">The POP3 server options.</param>
    /// <param name="serviceProvider">The service provider to use when resolving services.</param>
    /// <param name="logger">The logger to use.</param>
    public Pop3ServerBackgroundService(
      IPop3ServerOptions options,
      IServiceProvider serviceProvider,
      ILogger<Pop3ServerBackgroundService> logger)
    {
      _options = options;
      _serviceProvider = serviceProvider;
      _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      _logger.LogDebug($"{nameof(Pop3ServerBackgroundService)} is starting.");
      try
      {
        var pop3Server = new Pop3Server(_options, _serviceProvider);

        _logger.LogDebug($"{nameof(Pop3ServerBackgroundService)} is started.");

        stoppingToken.Register(() =>
          _logger.LogDebug($"{nameof(Pop3ServerBackgroundService)} is stopping."));

        await pop3Server.StartAsync(stoppingToken);

        _logger.LogDebug($"{nameof(Pop3ServerBackgroundService)} is stopped.");
      }
      catch (Exception exception)
      {
          _logger.LogError(exception, $"Exception occurred in {nameof(Pop3ServerBackgroundService)}.");
          throw;
      }
    }
  }
  
}
