using System.Buffers;
using System.IO.Pipelines;
using System.Text;
using Pop3Server.Mail;
using Pop3Server.Protocol;
using Pop3Server.Storage;
using Pop3Server.Test.Helpers;

namespace Pop3Server.Test
{
  public class TopCommandTests
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

    private int CountBodyLines(string output)
    {
      // Remove the "+OK message follows" line at the beginning
      var lines = output.Split("\r\n");

      // Find the empty line that separates headers from body
      var emptyLineIndex = -1;
      var inHeaders = true;
      var bodyLineCount = 0;

      for (int i = 1; i < lines.Length; i++) // Start at 1 to skip "+OK"
      {
        if (inHeaders && string.IsNullOrEmpty(lines[i]))
        {
          inHeaders = false;
          emptyLineIndex = i;
          continue;
        }

        if (!inHeaders)
        {
          // Stop at end marker ".",
          // note: "." could also appear in the body lines themselves (e.g., in file attachments)
          if (lines[i] == ".")
            break;

          bodyLineCount++;
        }
      }

      return bodyLineCount;
    }

    #region ExecuteAsync Tests

    [Fact]
    public async Task ExecuteAsync_ValidMessage_ReturnsOkResponse()
    {
      // Arrange
      var testMessage = new TestPop3Message { Id = "1", Size = 100, DeleteRequested = false };
      var messageStore = new TestMessageStore();
      messageStore.AddMessage(testMessage, CreateTestMessage("Subject: Test", "Body content"));

      var context = CreateContext(messageStore);
      context.Transaction.Mailbox = new Mailbox("testuser", "test.com");
      context.Transaction.Messages.Add(testMessage);

      var command = new TopCommand(1, 5);

      // Act
      var result = await command.ExecuteAsync(context, CancellationToken.None);

      // Assert
      Assert.True(result);

      var reader = context.Pipe.Input;
      var readResult = await reader.ReadAsync();
      var output = Encoding.ASCII.GetString(readResult.Buffer.ToArray());
      reader.AdvanceTo(readResult.Buffer.End);

      Assert.Contains("+OK message follows", output);
      Assert.Contains("Subject: Test", output);
      Assert.Contains("\r\n.\r\n", output); // End marker

      // Verify: 1 body line present (less than the requested 5, since only 1 line exists in the body)
      var bodyLines = CountBodyLines(output);
      Assert.Equal(1, bodyLines);
    }

    [Fact]
    public async Task ExecuteAsync_MessageNumberLessThanOne_ReturnsError()
    {
      // Arrange
      var context = CreateContext();
      context.Transaction.Mailbox = new Mailbox("testuser", "test.com");

      var command = new TopCommand(0, 5);

      // Act
      var result = await command.ExecuteAsync(context, CancellationToken.None);

      // Assert
      Assert.False(result);

      var reader = context.Pipe.Input;
      var readResult = await reader.ReadAsync();
      var output = Encoding.ASCII.GetString(readResult.Buffer.ToArray());
      reader.AdvanceTo(readResult.Buffer.End);

      Assert.Contains("-ERR no such message", output);
    }

    [Fact]
    public async Task ExecuteAsync_MessageNumberGreaterThanCount_ReturnsError()
    {
      // Arrange
      var testMessage = new TestPop3Message { Id = "1", Size = 100, DeleteRequested = false };
      var context = CreateContext();
      context.Transaction.Mailbox = new Mailbox("testuser", "test.com");
      context.Transaction.Messages.Add(testMessage);

      var command = new TopCommand(5, 5); // Request message 5 when only 1 exists

      // Act
      var result = await command.ExecuteAsync(context, CancellationToken.None);

      // Assert
      Assert.False(result);

      var reader = context.Pipe.Input;
      var readResult = await reader.ReadAsync();
      var output = Encoding.ASCII.GetString(readResult.Buffer.ToArray());
      reader.AdvanceTo(readResult.Buffer.End);

      Assert.Contains("-ERR no such message", output);
    }

    [Fact]
    public async Task ExecuteAsync_DeletedMessage_ReturnsError()
    {
      // Arrange
      var testMessage = new TestPop3Message { Id = "1", Size = 100, DeleteRequested = true };
      var context = CreateContext();
      context.Transaction.Mailbox = new Mailbox("testuser", "test.com");
      context.Transaction.Messages.Add(testMessage);

      var command = new TopCommand(1, 5);

      // Act
      var result = await command.ExecuteAsync(context, CancellationToken.None);

      // Assert
      Assert.False(result);

      var reader = context.Pipe.Input;
      var readResult = await reader.ReadAsync();
      var output = Encoding.ASCII.GetString(readResult.Buffer.ToArray());
      reader.AdvanceTo(readResult.Buffer.End);

      Assert.Contains("-ERR no such message", output);
    }

    [Fact]
    public async Task ExecuteAsync_WithZeroLines_ReturnsHeadersOnly()
    {
      // Arrange
      var testMessage = new TestPop3Message { Id = "1", Size = 100, DeleteRequested = false };
      var messageStore = new TestMessageStore();
      messageStore.AddMessage(testMessage, CreateTestMessage(
          """
          Subject: Test
          From: test@test.com
          """,
          """
          Body Line 1
          Body Line 2
          """));

      var context = CreateContext(messageStore);
      context.Transaction.Mailbox = new Mailbox("testuser", "test.com");
      context.Transaction.Messages.Add(testMessage);

      var command = new TopCommand(1, 0);

      // Act
      var result = await command.ExecuteAsync(context, CancellationToken.None);

      // Assert
      Assert.True(result);

      var reader = context.Pipe.Input;
      var readResult = await reader.ReadAsync();
      var output = Encoding.ASCII.GetString(readResult.Buffer.ToArray());
      reader.AdvanceTo(readResult.Buffer.End);

      Assert.Contains("Subject: Test", output);
      Assert.Contains("From: test@test.com", output);
      Assert.DoesNotContain("Body Line 1", output);

      // Verify: Exactly 0 body lines
      var bodyLines = CountBodyLines(output);
      Assert.Equal(0, bodyLines);
    }

    [Fact]
    public async Task ExecuteAsync_WithLimitedLines_ReturnsHeadersAndLimitedBody()
    {
      // Arrange
      var testMessage = new TestPop3Message { Id = "1", Size = 100, DeleteRequested = false };
      var messageStore = new TestMessageStore();
      messageStore.AddMessage(testMessage, CreateTestMessage(
          "Subject: Test",
          "Line 1\r\nLine 2\r\nLine 3\r\nLine 4\r\nLine 5"));

      var context = CreateContext(messageStore);
      context.Transaction.Mailbox = new Mailbox("testuser", "test.com");
      context.Transaction.Messages.Add(testMessage);

      var command = new TopCommand(1, 2);

      // Act
      var result = await command.ExecuteAsync(context, CancellationToken.None);

      // Assert
      Assert.True(result);

      var reader = context.Pipe.Input;
      var readResult = await reader.ReadAsync();
      var output = Encoding.ASCII.GetString(readResult.Buffer.ToArray());
      reader.AdvanceTo(readResult.Buffer.End);

      Assert.Contains("Subject: Test", output);
      Assert.Contains("Line 1", output);
      Assert.Contains("Line 2", output);
      Assert.DoesNotContain("Line 3", output);

      // Verify: Exactly 2 body lines
      var bodyLines = CountBodyLines(output);
      Assert.Equal(2, bodyLines);
    }

    [Fact]
    public async Task ExecuteAsync_MultipartMime_ZeroLines_ReturnsOnlyHeaders()
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
                Content-Transfer-Encoding: quoted-printable
                Content-Disposition: inline

                Plain text email goes here!
                This is the fallback if email client does not support HTML

                --boundary-string
                Content-Type: text/html; charset="utf-8"
                Content-Transfer-Encoding: quoted-printable
                Content-Disposition: inline

                <h1>This is the HTML Section!</h1>
                <p>This is what displays in most modern email clients</p>

                --boundary-string--
                """;

      messageStore.AddMessage(testMessage, Encoding.ASCII.GetBytes(mimeMessage));

      var context = CreateContext(messageStore);
      context.Transaction.Mailbox = new Mailbox("testuser", "test.com");
      context.Transaction.Messages.Add(testMessage);

      var command = new TopCommand(1, 0);

      // Act
      var result = await command.ExecuteAsync(context, CancellationToken.None);

      // Assert
      Assert.True(result);

      var reader = context.Pipe.Input;
      var readResult = await reader.ReadAsync();
      var output = Encoding.ASCII.GetString(readResult.Buffer.ToArray());
      reader.AdvanceTo(readResult.Buffer.End);

      // All headers must be present
      Assert.Contains("From: sender@example.com", output);
      Assert.Contains("To: recipient@example.com", output);
      Assert.Contains("Subject: Multipart Email Example", output);
      Assert.Contains("Content-Type: multipart/alternative", output);

      // NO body lines (everything after the empty line)
      Assert.DoesNotContain("--boundary-string", output);
      Assert.DoesNotContain("Plain text email goes here!", output);
      Assert.DoesNotContain("<h1>This is the HTML Section!</h1>", output);

      // End marker must be present
      Assert.Contains("\r\n.\r\n", output);

      // Verify: Exactly 0 body lines
      var bodyLines = CountBodyLines(output);
      Assert.Equal(0, bodyLines);
    }

    [Fact]
    public async Task ExecuteAsync_MultipartMime_ThreeLines_ReturnsHeadersAndThreeBodyLines()
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
                Content-Transfer-Encoding: quoted-printable
                Content-Disposition: inline

                Plain text email goes here!
                This is the fallback if email client does not support HTML

                --boundary-string
                Content-Type: text/html; charset="utf-8"
                Content-Transfer-Encoding: quoted-printable
                Content-Disposition: inline

                <h1>This is the HTML Section!</h1>
                <p>This is what displays in most modern email clients</p>

                --boundary-string--
                """;

      messageStore.AddMessage(testMessage, Encoding.ASCII.GetBytes(mimeMessage));

      var context = CreateContext(messageStore);
      context.Transaction.Mailbox = new Mailbox("testuser", "test.com");
      context.Transaction.Messages.Add(testMessage);

      var command = new TopCommand(1, 3);

      // Act
      var result = await command.ExecuteAsync(context, CancellationToken.None);

      // Assert
      Assert.True(result);

      var reader = context.Pipe.Input;
      var readResult = await reader.ReadAsync();
      var output = Encoding.ASCII.GetString(readResult.Buffer.ToArray());
      reader.AdvanceTo(readResult.Buffer.End);

      // All headers must be present
      Assert.Contains("From: sender@example.com", output);
      Assert.Contains("To: recipient@example.com", output);
      Assert.Contains("Subject: Multipart Email Example", output);
      Assert.Contains("Content-Type: multipart/alternative", output);

      // First 3 body lines must be present
      Assert.Contains("--boundary-string", output);
      Assert.Contains("Content-Type: text/plain", output);
      Assert.Contains("Content-Transfer-Encoding: quoted-printable", output);

      // Line 4+ should NOT be present
      Assert.DoesNotContain("Content-Disposition: inline", output);
      Assert.DoesNotContain("Plain text email goes here!", output);
      Assert.DoesNotContain("<h1>This is the HTML Section!</h1>", output);

      // End marker must be present
      Assert.Contains("\r\n.\r\n", output);
    }

    [Fact]
    public async Task ExecuteAsync_MoreLinesThanAvailable_ReturnsAllAvailableLines()
    {
      // Arrange
      var testMessage = new TestPop3Message { Id = "1", Size = 100, DeleteRequested = false };
      var messageStore = new TestMessageStore();
      messageStore.AddMessage(testMessage, CreateTestMessage(
          "Subject: Test",
          "Line 1\r\nLine 2\r\nLine 3"));

      var context = CreateContext(messageStore);
      context.Transaction.Mailbox = new Mailbox("testuser", "test.com");
      context.Transaction.Messages.Add(testMessage);

      var command = new TopCommand(1, 10000);

      // Act
      var result = await command.ExecuteAsync(context, CancellationToken.None);

      // Assert
      Assert.True(result);

      var reader = context.Pipe.Input;
      var readResult = await reader.ReadAsync();
      var output = Encoding.ASCII.GetString(readResult.Buffer.ToArray());
      reader.AdvanceTo(readResult.Buffer.End);

      Assert.Contains("Subject: Test", output);
      Assert.Contains("Line 1", output);
      Assert.Contains("Line 2", output);
      Assert.Contains("Line 3", output);
      Assert.Contains("\r\n.\r\n", output);
    }

    [Fact]
    public void Constructor_SetsProperties()
    {
      // Act
      var command = new TopCommand(5, 10);

      // Assert
      Assert.Equal(5, command.Message);
      Assert.Equal(10, command.Lines);
      Assert.Equal("TOP", command.Name);
    }

    #endregion ExecuteAsync Tests
  }
}
