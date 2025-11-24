using Pop3Server.IO;
using System.IO.Pipelines;

namespace Pop3Server.Test.Helpers
{
  public class TestSecurableDuplexPipe : ISecurableDuplexPipe
  {
    public TestSecurableDuplexPipe(PipeReader reader, PipeWriter writer)
    {
      Input = reader;
      Output = writer;
    }

    public PipeReader Input { get; }
    public PipeWriter Output { get; }
    public bool IsSecure => false;

    public Task UpgradeAsync(System.Security.Cryptography.X509Certificates.X509Certificate certificate,
        System.Security.Authentication.SslProtocols protocols, CancellationToken cancellationToken = default)
    {
      throw new NotImplementedException();
    }

    public void Dispose()
    { }
  }
}
