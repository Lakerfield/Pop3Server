using System.Collections.Generic;
using System.Net;
using SmtpServer.Mail;

namespace SmtpServer.Protocol
{
    public class SmtpCommandFactory : ISmtpCommandFactory
    {
    /// <summary>
    /// Create a USER command.
    /// </summary>
    /// <param name="username">The username literal.</param>
    /// <returns>The USER command.</returns>
    public virtual SmtpCommand CreateUser(string username)
    {
      return new UserCommand(username);
    }

    /// <summary>
    /// Create a PASS command.
    /// </summary>
    /// <param name="password">The password literal.</param>
    /// <returns>The PASS command.</returns>
    public virtual SmtpCommand CreatePass(string password)
    {
      return new PassCommand(password);
    }

    /// <summary>
    /// Create a CAPA command.
    /// </summary>
    /// <returns>The CAPA command.</returns>
    public virtual SmtpCommand CreateCapa()
    {
      return new CapaCommand();
    }

    /// <summary>
    /// Create a STAT command.
    /// </summary>
    /// <returns>The STAT command.</returns>
    public virtual SmtpCommand CreateStat()
    {
      return new StatCommand();
    }

    /// <summary>
    /// Create a LIST command.
    /// </summary>
    /// <param name="message">The optional message literal.</param>
    /// <returns>The LIST command.</returns>
    public virtual SmtpCommand CreateList(int message)
    {
      return new ListCommand(message);
    }

    /// <summary>
    /// Create a RETR command.
    /// </summary>
    /// <param name="message">The message literal.</param>
    /// <returns>The RETR command.</returns>
    public virtual SmtpCommand CreateRetr(int message)
    {
      return new RetrCommand(message);
    }

    /// <summary>
    /// Create a DELE command.
    /// </summary>
    /// <param name="message">The message literal.</param>
    /// <returns>The DELE command.</returns>
    public virtual SmtpCommand CreateDele(int message)
    {
      return new DeleCommand(message);
    }

    /// <summary>
    /// Create a TOP command.
    /// </summary>
    /// <param name="message">The message literal.</param>
    /// <returns>The TOP command.</returns>
    public virtual SmtpCommand CreateTop(int message, int lines)
    {
      return new TopCommand(message, lines);
    }

    /// <summary>
    /// Create a Uidl command.
    /// </summary>
    /// <param name="message">The message literal.</param>
    /// <returns>The Uidl command.</returns>
    public virtual SmtpCommand CreateUidl(int message)
    {
      return new UidlCommand(message);
    }



        /// <summary>
        /// Create a QUIT command.
        /// </summary>
        /// <returns>The QUITcommand.</returns>
        public virtual SmtpCommand CreateQuit()
        {
            return new QuitCommand();
        }

        /// <summary>
        /// Create a NOOP command.
        /// </summary>
        /// <returns>The NOOP command.</returns>
        public virtual SmtpCommand CreateNoop()
        {
            return new NoopCommand();
        }

        /// <summary>
        /// Create a RSET command.
        /// </summary>
        /// <returns>The RSET command.</returns>
        public virtual SmtpCommand CreateRset()
        {
            return new RsetCommand();
        }

        /// <summary>
        /// Create a STLS command.
        /// </summary>
        /// <returns>The STLS command.</returns>
        public virtual SmtpCommand CreateStls()
        {
            return new StlsCommand();
        }

        /// <summary>
        /// Create a AUTH command.
        /// </summary>
        /// <param name="method">The authentication method.</param>
        /// <param name="parameter">The authentication parameter.</param>
        /// <returns>The AUTH command.</returns>
        public SmtpCommand CreateAuth(AuthenticationMethod method, string parameter)
        {
            return new AuthCommand(method, parameter);
        }

        /// <summary>
        /// Create a PROXY command.
        /// </summary>
        /// <param name="sourceEndpoint">The source endpoint.</param>
        /// <param name="destinationEndpoint">The destination endpoint.</param>
        /// <returns>The PROXY command.</returns>
        public virtual SmtpCommand CreateProxy(IPEndPoint sourceEndpoint, IPEndPoint destinationEndpoint)
        {
            return new ProxyCommand(sourceEndpoint, destinationEndpoint);
        }
    }
}
