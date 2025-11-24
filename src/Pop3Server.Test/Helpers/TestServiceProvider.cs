using Pop3Server.Storage;

namespace Pop3Server.Test.Helpers
{
  public class TestServiceProvider : IServiceProvider
  {
    private readonly IMessageStore _messageStore;

    public TestServiceProvider(IMessageStore messageStore = null)
    {
      _messageStore = messageStore ?? new TestMessageStore();
    }

    public object GetService(Type serviceType)
    {
      if (serviceType == typeof(IMessageStoreFactory))
      {
        return new TestMessageStoreFactory(_messageStore);
      }
      return null;
    }
  }
}
