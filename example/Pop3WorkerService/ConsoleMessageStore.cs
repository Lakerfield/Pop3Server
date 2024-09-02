using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Pop3Server;
using Pop3Server.Mail;
using Pop3Server.Protocol;
using Pop3Server.Storage;

namespace WorkerService
{
  public sealed class ConsoleMessageStore : MessageStore
  {
    private List<ExampleMessage> _messages = new List<ExampleMessage>();

    public ConsoleMessageStore()
    {
      _messages.Add(new ExampleMessage("unique-id1", """
Date: Mon, 02 Sep 2024 10:00:00 +0000
From: sender@example.com
To: test@example.com
Subject: Hello world
Message-ID: <unique-id1@example.com>
MIME-Version: 1.0
Content-Type: text/plain; charset=UTF-8

This is a test.
"""));

      _messages.Add(new ExampleMessage("unique-id2", """
Date: Mon, 02 Sep 2024 11:00:00 +0000
From: another.sender@example.com
To: test@example.com
Subject: Hello world again
Message-ID: <unique-id2@example.com>
MIME-Version: 1.0
Content-Type: text/plain; charset=UTF-8

This is a another test.
"""));
    }

    public override Task<bool> LockMailboxAsync(ISessionContext context, IMailbox mailbox, CancellationToken cancellationToken)
    {
      return Task.FromResult(true);
    }

    public override Task UnlockMailboxAsync(ISessionContext context, IMailbox mailbox, CancellationToken cancellationToken)
    {
      return Task.FromResult(true);
    }

    public override Task<IPop3Message[]> GetMessagesAsync(ISessionContext context, IMailbox mailbox, CancellationToken cancellationToken)
    {
      return Task.FromResult(_messages.ToArray<IPop3Message>());
    }

    public override Task<byte[]> GetAsync(ISessionContext context, IMailbox mailbox, IPop3Message message, CancellationToken cancellationToken)
    {
      var exampleMessage = message as ExampleMessage;
      return Task.FromResult(exampleMessage.Data);
    }

    public override Task DeleteAsync(ISessionContext context, IMailbox mailbox, IPop3Message[] messages, CancellationToken cancellationToken)
    {
      return Task.FromResult(true);
    }


    public class ExampleMessage : IPop3Message
    {
      public string Id { get; set; }
      public int Size { get; set; }
      public bool DeleteRequested { get; set; }

      public byte[] Data { get; set; }
      public ExampleMessage(string id, string mail)
      {
        Id = id;
        Data = System.Text.Encoding.UTF8.GetBytes(mail);
        Size = Data.Length;
        DeleteRequested = false;
      }
    }
  }
}