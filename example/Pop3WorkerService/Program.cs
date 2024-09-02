using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pop3Server;
using Pop3Server.Authentication;
using Pop3Server.Storage;
using WorkerService;

namespace Pop3WorkerService
{
  public class Program
  {
    public static void Main(string[] args)
    {
      CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
      Host.CreateDefaultBuilder(args)
        .ConfigureServices(
          (hostContext, services) =>
          {
            services.AddTransient<IUserAuthenticator, ConsoleUserAuthenticator>();
            services.AddTransient<IMessageStore, ConsoleMessageStore>();

            services.AddSingleton(
              provider =>
              {
                var options = new Pop3ServerOptionsBuilder()
                  .ServerName("POP3 Server")
                  //.Port(9110)
                  .Endpoint(endpoint =>
                  {
                    endpoint
                      .Port(9110)
                      .AllowUnsecureAuthentication();
                  })
                  .Build();

                return new Pop3Server.Pop3Server(options, provider.GetRequiredService<IServiceProvider>());
              });

            services.AddHostedService<Worker>();
          });
  }

}
