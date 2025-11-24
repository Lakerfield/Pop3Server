namespace Pop3Server.Test.Helpers
{
  public class TestEndpointDefinition : IEndpointDefinition
  {
    public System.Net.IPEndPoint Endpoint => new System.Net.IPEndPoint(System.Net.IPAddress.Loopback, 110);
    public bool IsSecure => false;
    public bool AuthenticationRequired => true;
    public bool AllowUnsecureAuthentication => true;
    public TimeSpan ReadTimeout => TimeSpan.FromMinutes(1);

    public System.Security.Authentication.SslProtocols SupportedSslProtocols =>
        System.Security.Authentication.SslProtocols.Tls12;

    ICertificateFactory IEndpointDefinition.CertificateFactory => null;
  }
}
