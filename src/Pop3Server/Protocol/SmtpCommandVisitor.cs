using System;

namespace Pop3Server.Protocol
{
    public abstract class SmtpCommandVisitor
    {
        /// <summary>
        /// Visit the command.
        /// </summary>
        /// <param name="command"></param>
        public void Visit(SmtpCommand command)
        {
            if (command is AuthCommand authCommand)
            {
                Visit(authCommand);
                return;
            }


            if (command is UserCommand userCommand)
            {
                Visit(userCommand);
                return;
            }

            if (command is PassCommand passCommand)
            {
              Visit(passCommand);
              return;
            }

            if (command is CapaCommand capaCommand)
            {
              Visit(capaCommand);
              return;
            }

            if (command is StatCommand statCommand)
            {
              Visit(statCommand);
              return;
            }

            if (command is ListCommand listCommand)
            {
              Visit(listCommand);
              return;
            }

            if (command is RetrCommand retrCommand)
            {
              Visit(retrCommand);
              return;
            }

            if (command is DeleCommand deleCommand)
            {
              Visit(deleCommand);
              return;
            }

            if (command is TopCommand topCommand)
            {
              Visit(topCommand);
              return;
            }

            if (command is UidlCommand uidlCommand)
            {
                Visit(uidlCommand);
                return;
            }

            if (command is NoopCommand noopCommand)
            {
                Visit(noopCommand);
                return;
            }

            if (command is ProxyCommand proxyCommand)
            {
                Visit(proxyCommand);
                return;
            }

            if (command is QuitCommand quitCommand)
            {
                Visit(quitCommand);
                return;
            }

            if (command is RsetCommand rsetCommand)
            {
                Visit(rsetCommand);
                return;
            }

            if (command is StlsCommand tlsCommand)
            {
                Visit(tlsCommand);
                return;
            }

            throw new NotSupportedException(command.ToString());
        }

        /// <summary>
        /// Visit an AUTH command.
        /// </summary>
        /// <param name="command">The command that is being visited.</param>
        protected virtual void Visit(AuthCommand command) { }

        /// <summary>
        /// Visit an USER command.
        /// </summary>
        /// <param name="command">The command that is being visited.</param>
        protected virtual void Visit(UserCommand command) { }

        /// <summary>
        /// Visit an PASS command.
        /// </summary>
        /// <param name="command">The command that is being visited.</param>
        protected virtual void Visit(PassCommand command) { }

        /// <summary>
        /// Visit an CAPA command.
        /// </summary>
        /// <param name="command">The command that is being visited.</param>
        protected virtual void Visit(CapaCommand command) { }

        /// <summary>
        /// Visit an STAT command.
        /// </summary>
        /// <param name="command">The command that is being visited.</param>
        protected virtual void Visit(StatCommand command) { }

        /// <summary>
        /// Visit an LIST command.
        /// </summary>
        /// <param name="command">The command that is being visited.</param>
        protected virtual void Visit(ListCommand command) { }

        /// <summary>
        /// Visit an RETR command.
        /// </summary>
        /// <param name="command">The command that is being visited.</param>
        protected virtual void Visit(RetrCommand command) { }

        /// <summary>
        /// Visit an DELE command.
        /// </summary>
        /// <param name="command">The command that is being visited.</param>
        protected virtual void Visit(DeleCommand command) { }

        /// <summary>
        /// Visit an TOP command.
        /// </summary>
        /// <param name="command">The command that is being visited.</param>
        protected virtual void Visit(TopCommand command) { }

        /// <summary>
        /// Visit an UIDL command.
        /// </summary>
        /// <param name="command">The command that is being visited.</param>
        protected virtual void Visit(UidlCommand command) { }

        /// <summary>
        /// Visit an NOOP command.
        /// </summary>
        /// <param name="command">The command that is being visited.</param>
        protected virtual void Visit(NoopCommand command) { }

        /// <summary>
        /// Visit an PROXY command.
        /// </summary>
        /// <param name="command">The command that is being visited.</param>
        protected virtual void Visit(ProxyCommand command) { }

        /// <summary>
        /// Visit an QUIT command.
        /// </summary>
        /// <param name="command">The command that is being visited.</param>
        protected virtual void Visit(QuitCommand command) { }

        /// <summary>
        /// Visit an RSET command.
        /// </summary>
        /// <param name="command">The command that is being visited.</param>
        protected virtual void Visit(RsetCommand command) { }

        /// <summary>
        /// Visit an STLS command.
        /// </summary>
        /// <param name="command">The command that is being visited.</param>
        protected virtual void Visit(StlsCommand command) { }
    }
}
