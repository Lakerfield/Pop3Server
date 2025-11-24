namespace Pop3Server.Test.Helpers
{
  public class TestPop3ServerOptions : IPop3ServerOptions
  {
    public int MaxMessageSize => 10000000;
    public int MaxRetryCount => 3;
    public int MaxAuthenticationAttempts => 3;
    public string ServerName => "test.server";

    public IReadOnlyList<IEndpointDefinition> Endpoints =>
        new List<IEndpointDefinition>();

    public TimeSpan CommandWaitTimeout => TimeSpan.FromMinutes(1);
    public int NetworkBufferSize => 4096;
  }
}
