using SmtpServer.Mail;
using System.Collections.Generic;
using System.Net;

namespace SmtpServer.Protocol
{
    public interface ISmtpCommandFactory
    {
        /// <summary>
        /// Create a USER command.
        /// </summary>
        /// <param name="username">The username literal.</param>
        /// <returns>The USER command.</returns>
        SmtpCommand CreateUser(string username);

        /// <summary>
        /// Create a PASS command.
        /// </summary>
        /// <param name="password">The password literal.</param>
        /// <returns>The PASS command.</returns>
        SmtpCommand CreatePass(string password);

        /// <summary>
        /// Create a CAPA command.
        /// </summary>
        /// <returns>The CAPA command.</returns>
        SmtpCommand CreateCapa();

        /// <summary>
        /// Create a STAT command.
        /// </summary>
        /// <returns>The STAT command.</returns>
        SmtpCommand CreateStat();

        /// <summary>
        /// Create a LIST command.
        /// </summary>
        /// <param name="message">The optional message literal.</param>
        /// <returns>The LIST command.</returns>
        SmtpCommand CreateList(int message);

        /// <summary>
        /// Create a RETR command.
        /// </summary>
        /// <param name="message">The message literal.</param>
        /// <returns>The RETR command.</returns>
        SmtpCommand CreateRetr(int message);

        /// <summary>
        /// Create a DELE command.
        /// </summary>
        /// <param name="message">The message literal.</param>
        /// <returns>The DELE command.</returns>
        SmtpCommand CreateDele(int message);

        /// <summary>
        /// Create a TOP command.
        /// </summary>
        /// <param name="message">The message literal.</param>
        /// <returns>The TOP command.</returns>
        SmtpCommand CreateTop(int message, int lines);

        /// <summary>
        /// Create a Uidl command.
        /// </summary>
        /// <param name="message">The message literal.</param>
        /// <returns>The Uidl command.</returns>
        SmtpCommand CreateUidl(int message);

        /// <summary>
        /// Create a QUIT command.
        /// </summary>
        /// <returns>The QUIT command.</returns>
        SmtpCommand CreateQuit();

        /// <summary>
        /// Create a NOOP command.
        /// </summary>
        /// <returns>The NOOP command.</returns>
        SmtpCommand CreateNoop();

        /// <summary>
        /// Create a RSET command.
        /// </summary>
        /// <returns>The RSET command.</returns>
        SmtpCommand CreateRset();

        /// <summary>
        /// Create a STLS command.
        /// </summary>
        /// <returns>The STLS command.</returns>
        SmtpCommand CreateStls();

        /// <summary>
        /// Create a AUTH command.
        /// </summary>
        /// <param name="method">The authentication method.</param>
        /// <param name="parameter">The authentication parameter.</param>
        /// <returns>The AUTH command.</returns>
        SmtpCommand CreateAuth(AuthenticationMethod method, string parameter);

        /// <summary>
        /// Create a PROXY command.
        /// </summary>
        /// <param name="sourceEndpoint">The source endpoint.</param>
        /// <param name="destinationEndpoint">The destination endpoint.</param>
        /// <returns>The PROXY command.</returns>
        SmtpCommand CreateProxy(IPEndPoint sourceEndpoint = null, IPEndPoint destinationEndpoint = null);
    }
}
