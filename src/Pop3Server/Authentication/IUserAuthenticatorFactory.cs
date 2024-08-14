using Pop3Server.ComponentModel;

namespace Pop3Server.Authentication
{
    public interface IUserAuthenticatorFactory : ISessionContextInstanceFactory<IUserAuthenticator> { }
}
