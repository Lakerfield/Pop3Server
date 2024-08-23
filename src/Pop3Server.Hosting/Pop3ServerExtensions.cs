using System;
using Microsoft.Extensions.DependencyInjection;

namespace Pop3Server
{
  public static class Pop3ServerExtensions
  {
    public static IServiceCollection AddPop3Server(this IServiceCollection services, Action<Pop3ServerOptionsBuilder> configure)
    {
      var builder = new Pop3ServerOptionsBuilder();
      configure(builder);
      var options = builder.Build();

      services.AddSingleton<IPop3ServerOptions>(options);

      services.AddHostedService<Pop3ServerBackgroundService>();

      return services;
    }
  }

}
