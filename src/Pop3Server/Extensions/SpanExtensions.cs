using System;

namespace Pop3Server.Protocol
{
  public static class SpanExtensions
  {
    public static int IndexOfNewLine(this ReadOnlySpan<byte> span, char first = (char)13, char second = (char)10)
    {
      var maxLength = span.Length - 1;
      for (int i = 0; i < maxLength; i++)
        if (span[i] == first && span[i + 1] == second)
          return i;
      return -1;
    }

    public static int IndexOfPair(ReadOnlySpan<char> span, char first, char second)
    {
      for (int i = 0; i < span.Length - 1; i++)
        if (span[i] == first && span[i + 1] == second)
          return i;
      return -1;
    }
  }
}
