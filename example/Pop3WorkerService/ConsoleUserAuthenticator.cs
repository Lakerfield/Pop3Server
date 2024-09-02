using Pop3Server.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pop3Server;

namespace Pop3WorkerService
{
  public class ConsoleUserAuthenticator : IUserAuthenticator
  {
    public Task<bool> AuthenticateAsync(ISessionContext context, string user, string password, CancellationToken cancellationToken)
    {
      if (user == "test@example.com" && password == "test")
        return Task.FromResult(true);
      return Task.FromResult(false);
    }
  }
}
