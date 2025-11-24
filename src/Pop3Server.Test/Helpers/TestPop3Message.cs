namespace Pop3Server.Test.Helpers
{
  public class TestPop3Message : IPop3Message
  {
    public string Id { get; set; }
    public int Size { get; set; }
    public bool DeleteRequested { get; set; }
  }
}
