using System.Buffers;
using System.IO.Pipelines;
using System.Text;
using Pop3Server.Mail;
using Pop3Server.Protocol;
using Pop3Server.Storage;
using Pop3Server.Test.Helpers;

namespace Pop3Server.Test
{
  public partial class RetrCommandTests
  {
    private SmtpSessionContext CreateContext(IMessageStore messageStore = null)
    {
      var pipe = new Pipe();
      var serviceProvider = new TestServiceProvider(messageStore ?? new TestMessageStore());
      var options = new TestPop3ServerOptions();
      var endpointDefinition = new TestEndpointDefinition();

      var context = new SmtpSessionContext(serviceProvider, options, endpointDefinition)
      {
        Pipe = new TestSecurableDuplexPipe(pipe.Reader, pipe.Writer)
      };

      return context;
    }

    private byte[] CreateTestMessage(string headers, string body)
    {
      var message = $"""
                {headers}

                {body}
                """;
      return Encoding.ASCII.GetBytes(message);
    }

    #region ExecuteAsync Tests

    [Fact]
    public async Task ExecuteAsync_ValidMessage_ReturnsOkResponse()
    {
      // Arrange
      var testMessage = new TestPop3Message { Id = "1", Size = 100, DeleteRequested = false };
      var messageContent = CreateTestMessage("Subject: Test", "Body content");
      var messageStore = new TestMessageStore();
      messageStore.AddMessage(testMessage, messageContent);

      var context = CreateContext(messageStore);
      context.Transaction.Mailbox = new Mailbox("testuser", "test.com");
      context.Transaction.Messages.Add(testMessage);

      var command = new RetrCommand(1);

      // Act
      var result = await command.ExecuteAsync(context, CancellationToken.None);

      // Assert
      Assert.True(result);

      var reader = context.Pipe.Input;
      var readResult = await reader.ReadAsync();
      var output = Encoding.ASCII.GetString(readResult.Buffer.ToArray());
      reader.AdvanceTo(readResult.Buffer.End);

      var expected = $"""
        +OK message follows {messageContent.Length} octets
        Subject: Test

        Body content
        .

        """;
      Assert.Equal(expected, output);
    }

    [Fact]
    public async Task ExecuteAsync_ValidMessage_ReturnsCorrectSize()
    {
      // Arrange
      var testContent = CreateTestMessage("Subject: Test", "Body content");
      var testMessage = new TestPop3Message { Id = "1", Size = testContent.Length, DeleteRequested = false };
      var messageStore = new TestMessageStore();
      messageStore.AddMessage(testMessage, testContent);

      var context = CreateContext(messageStore);
      context.Transaction.Mailbox = new Mailbox("testuser", "test.com");
      context.Transaction.Messages.Add(testMessage);

      var command = new RetrCommand(1);

      // Act
      var result = await command.ExecuteAsync(context, CancellationToken.None);

      // Assert
      Assert.True(result);

      var reader = context.Pipe.Input;
      var readResult = await reader.ReadAsync();
      var output = Encoding.ASCII.GetString(readResult.Buffer.ToArray());
      reader.AdvanceTo(readResult.Buffer.End);

      var expected = $"""
        +OK message follows {testContent.Length} octets
        Subject: Test

        Body content
        .

        """;
      Assert.Equal(expected, output);
    }

    [Fact]
    public async Task ExecuteAsync_MessageNumberLessThanOne_ReturnsError()
    {
      // Arrange
      var context = CreateContext();
      context.Transaction.Mailbox = new Mailbox("testuser", "test.com");

      var command = new RetrCommand(0);

      // Act
      var result = await command.ExecuteAsync(context, CancellationToken.None);

      // Assert
      Assert.False(result);

      var reader = context.Pipe.Input;
      var readResult = await reader.ReadAsync();
      var output = Encoding.ASCII.GetString(readResult.Buffer.ToArray());
      reader.AdvanceTo(readResult.Buffer.End);

      var expected = """
        -ERR no such message

        """;
      Assert.Equal(expected, output);
    }

    [Fact]
    public async Task ExecuteAsync_MessageNumberGreaterThanCount_ReturnsError()
    {
      // Arrange
      var testMessage = new TestPop3Message { Id = "1", Size = 100, DeleteRequested = false };
      var context = CreateContext();
      context.Transaction.Mailbox = new Mailbox("testuser", "test.com");
      context.Transaction.Messages.Add(testMessage);

      var command = new RetrCommand(5); // Request message 5 when only 1 exists

      // Act
      var result = await command.ExecuteAsync(context, CancellationToken.None);

      // Assert
      Assert.False(result);

      var reader = context.Pipe.Input;
      var readResult = await reader.ReadAsync();
      var output = Encoding.ASCII.GetString(readResult.Buffer.ToArray());
      reader.AdvanceTo(readResult.Buffer.End);

      var expected = """
        -ERR no such message

        """;
      Assert.Equal(expected, output);
    }

    [Fact]
    public async Task ExecuteAsync_DeletedMessage_ReturnsError()
    {
      // Arrange
      var testMessage = new TestPop3Message { Id = "1", Size = 100, DeleteRequested = true };
      var context = CreateContext();
      context.Transaction.Mailbox = new Mailbox("testuser", "test.com");
      context.Transaction.Messages.Add(testMessage);

      var command = new RetrCommand(1);

      // Act
      var result = await command.ExecuteAsync(context, CancellationToken.None);

      // Assert
      Assert.False(result);

      var reader = context.Pipe.Input;
      var readResult = await reader.ReadAsync();
      var output = Encoding.ASCII.GetString(readResult.Buffer.ToArray());
      reader.AdvanceTo(readResult.Buffer.End);

      var expected = """
        -ERR no such message

        """;
      Assert.Equal(expected, output);
    }

    [Fact]
    public async Task ExecuteAsync_MessageWithDotAtLineStart_ReturnsDotStuffed()
    {
      // Arrange
      var testMessage = new TestPop3Message { Id = "1", Size = 100, DeleteRequested = false };
      var messageStore = new TestMessageStore();
      var messageContent = CreateTestMessage(
          "Subject: Test",
          """
          Line 1
          .This line starts with a dot
          Line 3
          """);
      messageStore.AddMessage(testMessage, messageContent);

      var context = CreateContext(messageStore);
      context.Transaction.Mailbox = new Mailbox("testuser", "test.com");
      context.Transaction.Messages.Add(testMessage);

      var command = new RetrCommand(1);

      // Act
      var result = await command.ExecuteAsync(context, CancellationToken.None);

      // Assert
      Assert.True(result);

      var reader = context.Pipe.Input;
      var readResult = await reader.ReadAsync();
      var output = Encoding.ASCII.GetString(readResult.Buffer.ToArray());
      reader.AdvanceTo(readResult.Buffer.End);

      var expected = $"""
        +OK message follows {messageContent.Length} octets
        Subject: Test

        Line 1
        ..This line starts with a dot
        Line 3
        .

        """;
      Assert.Equal(expected, output);
    }

    [Fact]
    public async Task ExecuteAsync_MessageWithMultipleDotLines_ReturnsAllDotStuffed()
    {
      // Arrange
      var testMessage = new TestPop3Message { Id = "1", Size = 100, DeleteRequested = false };
      var messageStore = new TestMessageStore();
      var messageContent = CreateTestMessage(
          "Subject: Test",
          """
          .First dot line
          Normal line
          .Second dot line
          .Third dot line
          """);
      messageStore.AddMessage(testMessage, messageContent);

      var context = CreateContext(messageStore);
      context.Transaction.Mailbox = new Mailbox("testuser", "test.com");
      context.Transaction.Messages.Add(testMessage);

      var command = new RetrCommand(1);

      // Act
      var result = await command.ExecuteAsync(context, CancellationToken.None);

      // Assert
      Assert.True(result);

      var reader = context.Pipe.Input;
      var readResult = await reader.ReadAsync();
      var output = Encoding.ASCII.GetString(readResult.Buffer.ToArray());
      reader.AdvanceTo(readResult.Buffer.End);

      var expected = $"""
        +OK message follows {messageContent.Length} octets
        Subject: Test

        ..First dot line
        Normal line
        ..Second dot line
        ..Third dot line
        .

        """;
      Assert.Equal(expected, output);
    }

    [Fact]
    public async Task ExecuteAsync_CompleteMessage_ReturnsHeadersAndBody()
    {
      // Arrange
      var testMessage = new TestPop3Message { Id = "1", Size = 100, DeleteRequested = false };
      var messageStore = new TestMessageStore();
      var messageContent = CreateTestMessage(
          """
          From: sender@example.com
          To: recipient@example.com
          Subject: Complete Message Test
          """,
          """
          This is the complete body.
          It has multiple lines.
          All should be returned.
          """);
      messageStore.AddMessage(testMessage, messageContent);

      var context = CreateContext(messageStore);
      context.Transaction.Mailbox = new Mailbox("testuser", "test.com");
      context.Transaction.Messages.Add(testMessage);

      var command = new RetrCommand(1);

      // Act
      var result = await command.ExecuteAsync(context, CancellationToken.None);

      // Assert
      Assert.True(result);

      var reader = context.Pipe.Input;
      var readResult = await reader.ReadAsync();
      var output = Encoding.ASCII.GetString(readResult.Buffer.ToArray());
      reader.AdvanceTo(readResult.Buffer.End);

      var expected = $"""
        +OK message follows {messageContent.Length} octets
        From: sender@example.com
        To: recipient@example.com
        Subject: Complete Message Test

        This is the complete body.
        It has multiple lines.
        All should be returned.
        .

        """;
      Assert.Equal(expected, output);
    }

    [Fact]
    public async Task ExecuteAsync_MultipartMimeMessage_ReturnsCompleteMessage()
    {
      // Arrange
      var testMessage = new TestPop3Message { Id = "1", Size = 500, DeleteRequested = false };
      var messageStore = new TestMessageStore();

      var mimeMessage = """
                From: sender@example.com
                To: recipient@example.com
                Subject: Multipart Email Example
                Content-Type: multipart/alternative; boundary="boundary-string"

                --boundary-string
                Content-Type: text/plain; charset="utf-8"

                Plain text email goes here!
                This is the fallback if email client does not support HTML

                --boundary-string
                Content-Type: text/html; charset="utf-8"

                <h1>This is the HTML Section!</h1>
                <p>This is what displays in most modern email clients</p>

                --boundary-string--
                """;

      var messageContent = Encoding.ASCII.GetBytes(mimeMessage);
      messageStore.AddMessage(testMessage, messageContent);

      var context = CreateContext(messageStore);
      context.Transaction.Mailbox = new Mailbox("testuser", "test.com");
      context.Transaction.Messages.Add(testMessage);

      var command = new RetrCommand(1);

      // Act
      var result = await command.ExecuteAsync(context, CancellationToken.None);

      // Assert
      Assert.True(result);

      var reader = context.Pipe.Input;
      var readResult = await reader.ReadAsync();
      var output = Encoding.ASCII.GetString(readResult.Buffer.ToArray());
      reader.AdvanceTo(readResult.Buffer.End);

      var expected = $"""
        +OK message follows {messageContent.Length} octets
        From: sender@example.com
        To: recipient@example.com
        Subject: Multipart Email Example
        Content-Type: multipart/alternative; boundary="boundary-string"

        --boundary-string
        Content-Type: text/plain; charset="utf-8"

        Plain text email goes here!
        This is the fallback if email client does not support HTML

        --boundary-string
        Content-Type: text/html; charset="utf-8"

        <h1>This is the HTML Section!</h1>
        <p>This is what displays in most modern email clients</p>

        --boundary-string--
        .

        """;
      Assert.Equal(expected, output);
    }

    [Fact]
    public void Constructor_SetsProperties()
    {
      // Act
      var command = new RetrCommand(5);

      // Assert
      Assert.Equal(5, command.Message);
      Assert.Equal("RETR", command.Name);
    }

    #endregion ExecuteAsync Tests
  }
}
