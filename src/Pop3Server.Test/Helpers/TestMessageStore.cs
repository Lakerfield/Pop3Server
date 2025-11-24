using Pop3Server.Mail;
using Pop3Server.Storage;

namespace Pop3Server.Test.Helpers
{
  public class TestMessageStore : IMessageStore
  {
    private readonly Dictionary<IPop3Message, byte[]> _messages = new Dictionary<IPop3Message, byte[]>();

    public void AddMessage(IPop3Message message, byte[] content)
    {
      _messages[message] = content;
    }

    public Task<byte[]> GetAsync(ISessionContext context, IMailbox mailbox, IPop3Message message, CancellationToken cancellationToken)
    {
      if (_messages.TryGetValue(message, out var content))
      {
        return Task.FromResult(content);
      }
      return Task.FromResult(new byte[0]);
    }

    public Task<IPop3Message[]> GetMessagesAsync(ISessionContext context, IMailbox mailbox, CancellationToken cancellationToken)
    {
      return Task.FromResult(Array.Empty<IPop3Message>());
    }

    public Task DeleteAsync(ISessionContext context, IMailbox mailbox, IPop3Message[] messages, CancellationToken cancellationToken)
    {
      return Task.CompletedTask;
    }

    public Task<bool> LockMailboxAsync(ISessionContext context, IMailbox mailbox, CancellationToken cancellationToken)
    {
      return Task.FromResult(true);
    }

    public Task UnlockMailboxAsync(ISessionContext context, IMailbox mailbox, CancellationToken cancellationToken)
    {
      return Task.CompletedTask;
    }
  }
}
