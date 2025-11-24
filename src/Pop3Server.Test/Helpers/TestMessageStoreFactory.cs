using Pop3Server.Storage;

namespace Pop3Server.Test.Helpers
{
  public class TestMessageStoreFactory : IMessageStoreFactory
  {
    private readonly IMessageStore _messageStore;

    public TestMessageStoreFactory(IMessageStore messageStore)
    {
      _messageStore = messageStore;
    }

    public IMessageStore CreateInstance(ISessionContext context)
    {
      return _messageStore;
    }
  }
}
