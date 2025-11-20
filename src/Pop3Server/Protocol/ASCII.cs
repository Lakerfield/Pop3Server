namespace Pop3Server.Protocol
{
  public static class ASCII
  {
    /// <summary>
    /// Carriage return (CR)
    /// </summary>
    public const byte CR = 13;

    /// <summary>
    /// Line feed (LF)
    /// </summary>
    public const byte LF = 10;

    /// <summary>
    /// dot (.)
    /// </summary>
    public const byte Dot = 46;

    public static readonly byte[] DotArray = new byte[] { ASCII.Dot };
    public static readonly byte[] CrLfArray = new byte[] { ASCII.CR, ASCII.LF };
    public const string NewLine = "\r\n";
    public const int NewLineLength = 2;
  }
}
