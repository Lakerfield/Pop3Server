namespace Pop3Server.Mail
{
  public interface IMailbox
  {
    /// <summary>
    /// Gets the user/account name.
    /// </summary>
    string User { get; }

    /// <summary>
    /// Gets the host server.
    /// </summary>
    string Host { get; }

    /// <summary>
    /// Gets the full email address.
    /// </summary>
    string Address { get; }
  }
}
