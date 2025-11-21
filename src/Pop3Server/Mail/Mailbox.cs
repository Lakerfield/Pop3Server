namespace Pop3Server.Mail
{
  public sealed class Mailbox : IMailbox
  {
    public static readonly IMailbox Empty = new Mailbox(string.Empty, string.Empty);

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="user">The user/account name.</param>
    /// <param name="host">The host server.</param>
    public Mailbox(string user, string host)
    {
      User = user ?? string.Empty;
      Host = host ?? string.Empty;
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="address">The email address to create the mailbox from.</param>
    public Mailbox(string address)
    {
      if (string.IsNullOrWhiteSpace(address))
      {
        User = string.Empty;
        Host = string.Empty;
        return;
      }
      var atIndex = address.IndexOf('@');
      if (atIndex >= 0)
      {
        User = address.Substring(0, atIndex);
        Host = address.Substring(atIndex + 1);
      }
      else
      {
        User = address;
        Host = string.Empty;
      }
    }

    /// <summary>
    /// Gets the user/account name.
    /// </summary>
    public string User { get; }

    /// <summary>
    /// Gets the host server.
    /// </summary>
    public string Host { get; }

    /// <summary>
    /// Gets the full email address.
    /// </summary>
    public string Address => string.IsNullOrEmpty(Host) ? User : $"{User}@{Host}";
  }
}
