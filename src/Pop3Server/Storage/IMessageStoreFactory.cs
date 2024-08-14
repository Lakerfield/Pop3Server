using Pop3Server.ComponentModel;

namespace Pop3Server.Storage
{
    public interface IMessageStoreFactory : ISessionContextInstanceFactory<IMessageStore> { }
}
