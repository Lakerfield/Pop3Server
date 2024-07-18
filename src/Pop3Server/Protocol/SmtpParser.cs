using System;
using System.Buffers;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using SmtpServer.IO;
using SmtpServer.Mail;
using SmtpServer.Text;

namespace SmtpServer.Protocol
{
    public sealed class SmtpParser
    {
        delegate bool TryMakeDelegate(ref TokenReader reader, out SmtpCommand command, out SmtpResponse errorResponse);

        static readonly SmtpResponse UnrecognizedCommand = new SmtpResponse(SmtpReplyCode.CommandNotImplemented, "Unrecognized command");

        readonly ISmtpCommandFactory _smtpCommandFactory;

        public SmtpParser(ISmtpCommandFactory smtpCommandFactory)
        {
            _smtpCommandFactory = smtpCommandFactory;
        }

        /// <summary>
        /// Make a command from the buffer.
        /// </summary>
        /// <param name="buffer">The buffer to read the command from.</param>
        /// <param name="command">The command that is defined within the token reader.</param>
        /// <param name="errorResponse">The error that indicates why the command could not be made.</param>
        /// <returns>Returns true if a command could be made, false if not.</returns>
        public bool TryMake(ref ReadOnlySequence<byte> buffer, out SmtpCommand command, out SmtpResponse errorResponse)
        {
      //USER PASS CAPA APOP STAT LIST RETR DELE TOP UIDL AUTH XTND
      //               APOP                              AUTH!XTND
      //USER PASS CAPA      STAT LIST RETR DELE TOP UIDL


      return Make(buffer, TryMakeUser, out command, out errorResponse)
                || Make(buffer, TryMakePass, out command, out errorResponse)
                || Make(buffer, TryMakeCapa, out command, out errorResponse)
                || Make(buffer, TryMakeStat, out command, out errorResponse)
                || Make(buffer, TryMakeList, out command, out errorResponse)
                || Make(buffer, TryMakeRetr, out command, out errorResponse)
                || Make(buffer, TryMakeDele, out command, out errorResponse)
                || Make(buffer, TryMakeTop, out command, out errorResponse)
                || Make(buffer, TryMakeUidl, out command, out errorResponse)
                || Make(buffer, TryMakeQuit, out command, out errorResponse)
                || Make(buffer, TryMakeRset, out command, out errorResponse)//
                || Make(buffer, TryMakeNoop, out command, out errorResponse)//
                || Make(buffer, TryMakeStls, out command, out errorResponse)//
                || Make(buffer, TryMakeAuth, out command, out errorResponse)
                || Make(buffer, TryMakeProxy, out command, out errorResponse)
                || Make(buffer, MakeUnrecognized, out command, out errorResponse);

            static bool Make(ReadOnlySequence<byte> buffer, TryMakeDelegate tryMakeDelegate, out SmtpCommand command, out SmtpResponse errorResponse)
            {
                var reader = new TokenReader(buffer);

                return tryMakeDelegate(ref reader, out command, out errorResponse);
            }
        }

        static bool MakeUnrecognized(ref TokenReader reader, out SmtpCommand command, out SmtpResponse errorResponse)
        {
            command = null;
            errorResponse = UnrecognizedCommand;

            return false;
        }



        /// <summary>
        /// Make a QUIT command.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <param name="command">The QUIT command that is defined within the token enumerator.</param>
        /// <param name="errorResponse">The error that indicates why the command could not be made.</param>
        /// <returns>Returns true if a command could be made, false if not.</returns>
        public bool TryMakeQuit(ref TokenReader reader, out SmtpCommand command, out SmtpResponse errorResponse)
        {
            command = null;
            errorResponse = null;

            if (reader.TryMake(TryMakeQuitLiteral) == false)
            {
                return false;
            }

            if (TryMakeEnd(ref reader) == false)
            {
                errorResponse = SmtpResponse.SyntaxError;
                return false;
            }

            command = _smtpCommandFactory.CreateQuit();
            return true;
        }

        /// <summary>
        /// Try to make the QUIT text sequence.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <returns>true if the QUIT text sequence could be made, false if not.</returns>
        public bool TryMakeQuitLiteral(ref TokenReader reader)
        {
            if (reader.TryMake(TryMakeText, out var text))
            {
                Span<char> command = stackalloc char[4];
                command[0] = 'Q';
                command[1] = 'U';
                command[2] = 'I';
                command[3] = 'T';

                return text.CaseInsensitiveStringEquals(ref command);
            }

            return false;
        }

        /// <summary>
        /// Make a NOOP command.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <param name="command">The NOOP command that is defined within the token enumerator.</param>
        /// <param name="errorResponse">The error that indicates why the command could not be made.</param>
        /// <returns>Returns true if a command could be made, false if not.</returns>
        public bool TryMakeNoop(ref TokenReader reader, out SmtpCommand command, out SmtpResponse errorResponse)
        {
            command = null;
            errorResponse = null;

            if (reader.TryMake(TryMakeNoopLiteral) == false)
            {
                return false;
            }

            if (TryMakeEnd(ref reader) == false)
            {
                errorResponse = SmtpResponse.SyntaxError;
                return false;
            }

            command = _smtpCommandFactory.CreateNoop();
            return true;
        }

        /// <summary>
        /// Try to make the NOOP text sequence.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <returns>true if the NOOP text sequence could be made, false if not.</returns>
        public bool TryMakeNoopLiteral(ref TokenReader reader)
        {
            if (reader.TryMake(TryMakeText, out var text))
            {
                Span<char> command = stackalloc char[4];
                command[0] = 'N';
                command[1] = 'O';
                command[2] = 'O';
                command[3] = 'P';

                return text.CaseInsensitiveStringEquals(ref command);
            }

            return false;
        }

        /// <summary>
        /// Make a RSET command.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <param name="command">The RSET command that is defined within the token enumerator.</param>
        /// <param name="errorResponse">The error that indicates why the command could not be made.</param>
        /// <returns>Returns true if a command could be made, false if not.</returns>
        public bool TryMakeRset(ref TokenReader reader, out SmtpCommand command, out SmtpResponse errorResponse)
        {
            command = null;
            errorResponse = null;

            if (reader.TryMake(TryMakeRsetLiteral) == false)
            {
                return false;
            }

            reader.Skip(TokenKind.Space);

            if (TryMakeEnd(ref reader) == false)
            {
                errorResponse = SmtpResponse.SyntaxError;
                return false;
            }

            command = _smtpCommandFactory.CreateRset();
            return true;
        }

        /// <summary>
        /// Try to make the RSET text sequence.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <returns>true if the RSET text sequence could be made, false if not.</returns>
        public bool TryMakeRsetLiteral(ref TokenReader reader)
        {
            if (reader.TryMake(TryMakeText, out var text))
            {
                Span<char> command = stackalloc char[4];
                command[0] = 'R';
                command[1] = 'S';
                command[2] = 'E';
                command[3] = 'T';

                return text.CaseInsensitiveStringEquals(ref command);
            }

            return false;
        }

        /// <summary>
        /// Make an STLS command from the given enumerator.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <param name="command">The STLS command that is defined within the token enumerator.</param>
        /// <param name="errorResponse">The error that indicates why the command could not be made.</param>
        /// <returns>Returns true if a command could be made, false if not.</returns>
        public bool TryMakeStls(ref TokenReader reader, out SmtpCommand command, out SmtpResponse errorResponse)
        {
            command = null;
            errorResponse = null;

            if (reader.TryMake(TryMakeStlsLiteral) == false)
            {
                return false;
            }

            reader.Skip(TokenKind.Space);

            if (TryMakeEnd(ref reader) == false)
            {
                errorResponse = SmtpResponse.SyntaxError;
                return false;
            }

            command = _smtpCommandFactory.CreateStls();
            return true;
        }

        /// <summary>
        /// Try to make the STLS text sequence.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <returns>true if the STLS text sequence could be made, false if not.</returns>
        public bool TryMakeStlsLiteral(ref TokenReader reader)
        {
            if (reader.TryMake(TryMakeText, out var text))
            {
                Span<char> command = stackalloc char[4];
                command[0] = 'S';
                command[1] = 'T';
                command[2] = 'L';
                command[3] = 'S';

                return text.CaseInsensitiveStringEquals(ref command);
            }

            return false;
        }

        /// <summary>
        /// Make an USER command from the given enumerator.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <param name="mailbox">The USER mailbox that is defined within the token enumerator.</param>
        /// <param name="errorResponse">The error that indicates why the command could not be made.</param>
        /// <returns>Returns true if a command could be made, false if not.</returns>
        public bool TryMakeUser(ref TokenReader reader, out SmtpCommand command, out SmtpResponse errorResponse)
        {
            command = null;
            errorResponse = null;
            IMailbox mailbox;

            if (reader.TryMake(TryMakeUserLiteral) == false)
            {
                return false;
            }

            reader.Skip(TokenKind.Space);

            if (reader.TryMake(TryMakeSomethingText, out var username))
            {
              command = _smtpCommandFactory.CreateUser(StringUtil.Create(username));
              return true;
            }

            errorResponse = SmtpResponse.SyntaxError;
            return false;
        }

        /// <summary>
        /// Try to make the USER text sequence.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <returns>true if the USER text sequence could be made, false if not.</returns>
        public bool TryMakeUserLiteral(ref TokenReader reader)
        {
            if (reader.TryMake(TryMakeText, out var text))
            {
                Span<char> command = stackalloc char[4];
                command[0] = 'U';
                command[1] = 'S';
                command[2] = 'E';
                command[3] = 'R';

                return text.CaseInsensitiveStringEquals(ref command);
            }

            return false;
        }

        /// <summary>
        /// Make an PASS command from the given enumerator.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <param name="mailbox">The PASS mailbox that is defined within the token enumerator.</param>
        /// <param name="errorResponse">The error that indicates why the command could not be made.</param>
        /// <returns>Returns true if a command could be made, false if not.</returns>
        public bool TryMakePass(ref TokenReader reader, out SmtpCommand command, out SmtpResponse errorResponse)
        {
          command = null;
          errorResponse = null;

          if (reader.TryMake(TryMakePassLiteral) == false)
          {
            return false;
          }

          reader.Skip(TokenKind.Space);

          if (reader.TryMake(TryMakeSomethingText, out var password))
          {
            command = _smtpCommandFactory.CreatePass(StringUtil.Create(password));
            return true;
          }

          errorResponse = SmtpResponse.SyntaxError;
          return false;
        }

        /// <summary>
        /// Try to make the PASS text sequence.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <returns>true if the PASS text sequence could be made, false if not.</returns>
        public bool TryMakePassLiteral(ref TokenReader reader)
        {
          if (reader.TryMake(TryMakeText, out var text))
          {
            Span<char> command = stackalloc char[4];
            command[0] = 'P';
            command[1] = 'A';
            command[2] = 'S';
            command[3] = 'S';

            return text.CaseInsensitiveStringEquals(ref command);
          }

          return false;
        }

        /// <summary>
        /// Make an CAPA command from the given enumerator.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <param name="mailbox">The CAPA mailbox that is defined within the token enumerator.</param>
        /// <param name="errorResponse">The error that indicates why the command could not be made.</param>
        /// <returns>Returns true if a command could be made, false if not.</returns>
        public bool TryMakeCapa(ref TokenReader reader, out SmtpCommand command, out SmtpResponse errorResponse)
        {
          command = null;
          errorResponse = null;

          if (reader.TryMake(TryMakeCapaLiteral) == false)
          {
            return false;
          }

          command = _smtpCommandFactory.CreateCapa();
          return true;
        }

        /// <summary>
        /// Try to make the CAPA text sequence.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <returns>true if the CAPA text sequence could be made, false if not.</returns>
        public bool TryMakeCapaLiteral(ref TokenReader reader)
        {
          if (reader.TryMake(TryMakeText, out var text))
          {
            Span<char> command = stackalloc char[4];
            command[0] = 'C';
            command[1] = 'A';
            command[2] = 'P';
            command[3] = 'A';

            return text.CaseInsensitiveStringEquals(ref command);
          }

          return false;
        }


        /// <summary>
        /// Make an STAT command from the given enumerator.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <param name="mailbox">The STAT mailbox that is defined within the token enumerator.</param>
        /// <param name="errorResponse">The error that indicates why the command could not be made.</param>
        /// <returns>Returns true if a command could be made, false if not.</returns>
        public bool TryMakeStat(ref TokenReader reader, out SmtpCommand command, out SmtpResponse errorResponse)
        {
          command = null;
          errorResponse = null;

          if (reader.TryMake(TryMakeStatLiteral) == false)
          {
            return false;
          }

          command = _smtpCommandFactory.CreateStat();
          return true;
        }

        /// <summary>
        /// Try to make the STAT text sequence.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <returns>true if the STAT text sequence could be made, false if not.</returns>
        public bool TryMakeStatLiteral(ref TokenReader reader)
        {
          if (reader.TryMake(TryMakeText, out var text))
          {
            Span<char> command = stackalloc char[4];
            command[0] = 'S';
            command[1] = 'T';
            command[2] = 'A';
            command[3] = 'T';

            return text.CaseInsensitiveStringEquals(ref command);
          }

          return false;
        }

        /// <summary>
        /// Make an LIST command from the given enumerator.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <param name="mailbox">The LIST mailbox that is defined within the token enumerator.</param>
        /// <param name="errorResponse">The error that indicates why the command could not be made.</param>
        /// <returns>Returns true if a command could be made, false if not.</returns>
        public bool TryMakeList(ref TokenReader reader, out SmtpCommand command, out SmtpResponse errorResponse)
        {
          command = null;
          errorResponse = null;

          if (reader.TryMake(TryMakeListLiteral) == false)
          {
            return false;
          }

          reader.Skip(TokenKind.Space);

          if (reader.TryMake(TryMakeNumber, out var number))
          {
            int.TryParse(StringUtil.Create(number), out var num);
            command = _smtpCommandFactory.CreateList(num);
            return true;
          }

          errorResponse = SmtpResponse.SyntaxError;
          return false;
        }

        /// <summary>
        /// Try to make the LIST text sequence.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <returns>true if the LIST text sequence could be made, false if not.</returns>
        public bool TryMakeListLiteral(ref TokenReader reader)
        {
          if (reader.TryMake(TryMakeText, out var text))
          {
            Span<char> command = stackalloc char[4];
            command[0] = 'L';
            command[1] = 'I';
            command[2] = 'S';
            command[3] = 'T';

            return text.CaseInsensitiveStringEquals(ref command);
          }

          return false;
        }

        /// <summary>
    /// Make an RETR command from the given enumerator.
    /// </summary>
    /// <param name="reader">The reader to perform the operation on.</param>
    /// <param name="mailbox">The RETR mailbox that is defined within the token enumerator.</param>
    /// <param name="errorResponse">The error that indicates why the command could not be made.</param>
    /// <returns>Returns true if a command could be made, false if not.</returns>
    public bool TryMakeRetr(ref TokenReader reader, out SmtpCommand command, out SmtpResponse errorResponse)
        {
          command = null;
          errorResponse = null;

          if (reader.TryMake(TryMakeRetrLiteral) == false)
          {
            return false;
          }

          reader.Skip(TokenKind.Space);

          if (reader.TryMake(TryMakeNumber, out var number))
          {
            int.TryParse(StringUtil.Create(number), out var num);
            command = _smtpCommandFactory.CreateRetr(num);
            return true;
          }

          errorResponse = SmtpResponse.SyntaxError;
          return false;
        }

        /// <summary>
        /// Try to make the RETR text sequence.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <returns>true if the RETR text sequence could be made, false if not.</returns>
        public bool TryMakeRetrLiteral(ref TokenReader reader)
        {
          if (reader.TryMake(TryMakeText, out var text))
          {
            Span<char> command = stackalloc char[4];
            command[0] = 'R';
            command[1] = 'E';
            command[2] = 'T';
            command[3] = 'R';

            return text.CaseInsensitiveStringEquals(ref command);
          }

          return false;
        }

        /// <summary>
        /// Make an DELE command from the given enumerator.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <param name="mailbox">The DELE mailbox that is defined within the token enumerator.</param>
        /// <param name="errorResponse">The error that indicates why the command could not be made.</param>
        /// <returns>Returns true if a command could be made, false if not.</returns>
        public bool TryMakeDele(ref TokenReader reader, out SmtpCommand command, out SmtpResponse errorResponse)
        {
          command = null;
          errorResponse = null;

          if (reader.TryMake(TryMakeDeleLiteral) == false)
          {
            return false;
          }

          reader.Skip(TokenKind.Space);

          if (reader.TryMake(TryMakeNumber, out var number))
          {
            int.TryParse(StringUtil.Create(number), out var num);
            command = _smtpCommandFactory.CreateDele(num);
            return true;
          }

          errorResponse = SmtpResponse.SyntaxError;
          return false;
        }

        /// <summary>
        /// Try to make the DELE text sequence.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <returns>true if the DELE text sequence could be made, false if not.</returns>
        public bool TryMakeDeleLiteral(ref TokenReader reader)
        {
          if (reader.TryMake(TryMakeText, out var text))
          {
            Span<char> command = stackalloc char[4];
            command[0] = 'D';
            command[1] = 'E';
            command[2] = 'L';
            command[3] = 'E';

            return text.CaseInsensitiveStringEquals(ref command);
          }

          return false;
        }

        /// <summary>
        /// Make an TOP command from the given enumerator.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <param name="mailbox">The TOP mailbox that is defined within the token enumerator.</param>
        /// <param name="errorResponse">The error that indicates why the command could not be made.</param>
        /// <returns>Returns true if a command could be made, false if not.</returns>
        public bool TryMakeTop(ref TokenReader reader, out SmtpCommand command, out SmtpResponse errorResponse)
        {
          command = null;
          errorResponse = null;

          if (reader.TryMake(TryMakeTopLiteral) == false)
          {
            return false;
          }

          reader.Skip(TokenKind.Space);

          int num = 0;
          if (reader.TryMake(TryMakeNumber, out var number))
          {
            int.TryParse(StringUtil.Create(number), out num);
          }
          else
          {
            errorResponse = SmtpResponse.SyntaxError;
            return false;
          }

          if (reader.TryMake(TryMakeNumber, out var number2) == false)
          {
            command = _smtpCommandFactory.CreateTop(num, 0);
            return true;
          }
          else
          {
            int.TryParse(StringUtil.Create(number), out var num2);
            command = _smtpCommandFactory.CreateTop(num, num2);
            return true;
          }
        }

        /// <summary>
        /// Try to make the TOP text sequence.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <returns>true if the TOP text sequence could be made, false if not.</returns>
        public bool TryMakeTopLiteral(ref TokenReader reader)
        {
          if (reader.TryMake(TryMakeText, out var text))
          {
            Span<char> command = stackalloc char[3];
            command[0] = 'T';
            command[1] = 'O';
            command[2] = 'P';

            return text.CaseInsensitiveStringEquals(ref command);
          }

          return false;
        }

        /// <summary>
        /// Make an UIDL command from the given enumerator.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <param name="mailbox">The UIDL mailbox that is defined within the token enumerator.</param>
        /// <param name="errorResponse">The error that indicates why the command could not be made.</param>
        /// <returns>Returns true if a command could be made, false if not.</returns>
        public bool TryMakeUidl(ref TokenReader reader, out SmtpCommand command, out SmtpResponse errorResponse)
        {
          command = null;
          errorResponse = null;

          if (reader.TryMake(TryMakeUidlLiteral) == false)
          {
            return false;
          }

          reader.Skip(TokenKind.Space);

          if (reader.TryMake(TryMakeNumber, out var number))
          {
            int.TryParse(StringUtil.Create(number), out var num);
            command = _smtpCommandFactory.CreateUidl(num);
            return true;
          }

          errorResponse = SmtpResponse.SyntaxError;
          return false;
        }

        /// <summary>
        /// Try to make the UIDL text sequence.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <returns>true if the UIDL text sequence could be made, false if not.</returns>
        public bool TryMakeUidlLiteral(ref TokenReader reader)
        {
          if (reader.TryMake(TryMakeText, out var text))
          {
            Span<char> command = stackalloc char[4];
            command[0] = 'U';
            command[1] = 'I';
            command[2] = 'D';
            command[3] = 'L';

            return text.CaseInsensitiveStringEquals(ref command);
          }

          return false;
        }





    /// <summary>
    /// Make an AUTH command from the given enumerator.
    /// </summary>
    /// <param name="reader">The reader to perform the operation on.</param>
    /// <param name="command">The AUTH command that is defined within the token enumerator.</param>
    /// <param name="errorResponse">The error that indicates why the command could not be made.</param>
    /// <returns>Returns true if a command could be made, false if not.</returns>
    public bool TryMakeAuth(ref TokenReader reader, out SmtpCommand command, out SmtpResponse errorResponse)
        {
            command = null;
            errorResponse = null;

            if (reader.TryMake(TryMakeAuthLiteral) == false)
            {
                return false;
            }

            reader.Skip(TokenKind.Space);

            if (TryMakeAuthenticationMethod(ref reader, out var authenticationMethod) == false)
            {
                return false;
            }

            reader.Take();

            if (reader.TryMake(TryMakeEnd))
            {
                command = _smtpCommandFactory.CreateAuth(authenticationMethod, null);
                return true;
            }

            if (reader.TryMake(TryMakeBase64, out var base64))
            {
                command = _smtpCommandFactory.CreateAuth(authenticationMethod, StringUtil.Create(base64));
                return true;
            }

            errorResponse = SmtpResponse.SyntaxError;
            return false;
        }

        /// <summary>
        /// Try to make the Authentication method.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <param name="authenticationMethod">The authentication method that was made.</param>
        /// <returns>true if the authentication method could be made, false if not.</returns>
        public bool TryMakeAuthenticationMethod(ref TokenReader reader, out AuthenticationMethod authenticationMethod)
        {
            if (reader.TryMake(TryMakeLoginLiteral))
            {
                authenticationMethod = AuthenticationMethod.Login;
                return true;
            }

            if (reader.TryMake(TryMakePlainLiteral))
            {
                authenticationMethod = AuthenticationMethod.Plain;
                return true;
            }

            authenticationMethod = default;
            return false;
        }

        /// <summary>
        /// Try to make the AUTH text sequence.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <returns>true if the AUTH text sequence could be made, false if not.</returns>
        public bool TryMakeAuthLiteral(ref TokenReader reader)
        {
            if (reader.TryMake(TryMakeText, out var text))
            {
                Span<char> command = stackalloc char[4];
                command[0] = 'A';
                command[1] = 'U';
                command[2] = 'T';
                command[3] = 'H';

                return text.CaseInsensitiveStringEquals(ref command);
            }

            return false;
        }

        /// <summary>
        /// Try to make the LOGIN text sequence.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <returns>true if the LOGIN text sequence could be made, false if not.</returns>
        public bool TryMakeLoginLiteral(ref TokenReader reader)
        {
            if (reader.TryMake(TryMakeText, out var text))
            {
                Span<char> command = stackalloc char[5];
                command[0] = 'L';
                command[1] = 'O';
                command[2] = 'G';
                command[3] = 'I';
                command[4] = 'N';

                return text.CaseInsensitiveStringEquals(ref command);
            }

            return false;
        }

        /// <summary>
        /// Try to make the PLAIN text sequence.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <returns>true if the PLAIN text sequence could be made, false if not.</returns>
        public bool TryMakePlainLiteral(ref TokenReader reader)
        {
            if (reader.TryMake(TryMakeText, out var text))
            {
                Span<char> command = stackalloc char[5];
                command[0] = 'P';
                command[1] = 'L';
                command[2] = 'A';
                command[3] = 'I';
                command[4] = 'N';

                return text.CaseInsensitiveStringEquals(ref command);
            }

            return false;
        }

        /// <summary>
        /// Support proxy protocol version 1 header for use with HAProxy.
        /// Documented at http://www.haproxy.org/download/1.8/doc/proxy-protocol.txt
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <param name="command">The PROXY command that is defined within the token enumerator.</param>
        /// <param name="errorResponse">The error that indicates why the command could not be made.</param>
        /// <returns>Returns true if a command could be made, false if not.</returns>
        public bool TryMakeProxy(ref TokenReader reader, out SmtpCommand command, out SmtpResponse errorResponse)
        {
            // ABNF
            // proxy            = "PROXY" space ( unknown-proxy | tcp4-proxy | tcp6-proxy )
            // unknown-proxy    = "UNKNOWN"
            // tcp4-proxy       = "TCP4" space ipv4-address-literal space ipv4-address-literal space ip-port-number space ip-port-number   
            // tcp6-proxy       = "TCP6" space ipv6-address-literal space ipv6-address-literal space ip-port-number space ip-port-number
            // space            = " "
            // ip-port          = wnum
            // wnum             = 1*5DIGIT ; in the range of 0-65535

            command = null;
            errorResponse = null;

            if (reader.TryMake(TryMakeProxyLiteral) == false)
            {
                return false;
            }

            reader.Skip(TokenKind.Space);

            if (reader.TryMake(TryMakeUnknownLiteral))
            {
                command = _smtpCommandFactory.CreateProxy();
                return true;
            }

            if (reader.TryMake(TryMakeTcp4Proxy, out command))
            {
                return true;
            }

            return TryMakeTcp6Proxy(ref reader, out command);
        }

        /// <summary>
        /// Try to make the PROXY text sequence.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <returns>true if the PROXY text sequence could be made, false if not.</returns>
        public bool TryMakeProxyLiteral(ref TokenReader reader)
        {
            if (reader.TryMake(TryMakeText, out var text))
            {
                Span<char> command = stackalloc char[5];
                command[0] = 'P';
                command[1] = 'R';
                command[2] = 'O';
                command[3] = 'X';
                command[4] = 'Y';

                return text.CaseInsensitiveStringEquals(ref command);
            }

            return false;
        }

        /// <summary>
        /// Attempt to make the Unknown Proxy command.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <returns>true if the command was made, false if not.</returns>
        bool TryMakeUnknownLiteral(ref TokenReader reader)
        {
            if (reader.TryMake(TryMakeText, out var text))
            {
                Span<char> command = stackalloc char[7];
                command[0] = 'U';
                command[1] = 'N';
                command[2] = 'K';
                command[3] = 'N';
                command[4] = 'O';
                command[5] = 'W';
                command[6] = 'N';

                return text.CaseInsensitiveStringEquals(ref command);
            }

            return false;
        }

        /// <summary>
        /// Attempt to make a TCP4 Proxy command.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <param name="command">The command that was made.</param>
        /// <returns>true if the command was made, false if not.</returns>
        public bool TryMakeTcp4Proxy(ref TokenReader reader, out SmtpCommand command)
        {
            command = null;

            if (TryMakeTcpLiteral(ref reader) == false)
            {
                return false;
            }

            var token = reader.Take();
            if (token.Kind != TokenKind.Number && token.Text[0] != '4')
            {
                return false;
            }

            return TryMakeProxyAddresses(ref reader, TryMakeIPv4AddressLiteral, out command);
        }

        /// <summary>
        /// Attempt to make a TCP6 Proxy command.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <param name="command">The command that was made.</param>
        /// <returns>true if the command was made, false if not.</returns>
        public bool TryMakeTcp6Proxy(ref TokenReader reader, out SmtpCommand command)
        {
            command = null;

            if (TryMakeTcpLiteral(ref reader) == false)
            {
                return false;
            }

            var token = reader.Take();
            if (token.Kind != TokenKind.Number && token.Text[0] != '6')
            {
                return false;
            }

            return TryMakeProxyAddresses(ref reader, TryMakeIPv6Address, out command);
        }

        /// <summary>
        /// Attempt to make the proxy address sequences.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <param name="tryMakeDelegate">The delegate to match the address.</param>
        /// <param name="command">The command that was made.</param>
        /// <returns>true if the command was made, false if not.</returns>
        bool TryMakeProxyAddresses(ref TokenReader reader, TokenReader.TryMakeDelegate tryMakeDelegate, out SmtpCommand command)
        {
            command = null;

            reader.Skip(TokenKind.Space);

            if (reader.TryMake(tryMakeDelegate, out var sourceAddress) == false)
            {
                return false;
            }

            reader.Skip(TokenKind.Space);

            if (reader.TryMake(tryMakeDelegate, out var destinationAddress) == false)
            {
                return false;
            }

            reader.Skip(TokenKind.Space);

            if (reader.TryMake(TryMakeWnum, out var sourcePort) == false)
            {
                return false;
            }

            reader.Skip(TokenKind.Space);

            if (reader.TryMake(TryMakeWnum, out var destinationPort) == false)
            {
                return false;
            }

            command = _smtpCommandFactory.CreateProxy(CreateEndpoint(sourceAddress, sourcePort), CreateEndpoint(destinationAddress, destinationPort));
            return true;

            static IPEndPoint CreateEndpoint(ReadOnlySequence<byte> address, ReadOnlySequence<byte> port)
            {
                return new IPEndPoint(IPAddress.Parse(StringUtil.Create(address)), int.Parse(StringUtil.Create(port)));
            }
        }

        /// <summary>
        /// Try to make the Tcp text sequence.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <returns>true if the TCP text sequence could be made, false if not.</returns>
        public bool TryMakeTcpLiteral(ref TokenReader reader)
        {
            if (reader.TryMake(TryMakeText, out var text))
            {
                Span<char> command = stackalloc char[3];
                command[0] = 'T';
                command[1] = 'C';
                command[2] = 'P';

                return text.CaseInsensitiveStringEquals(ref command);
            }

            return false;
        }

        /// <summary>
        /// Try to make the end of sequence.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <returns>true if the end was made, false if not.</returns>
        public bool TryMakeEnd(ref TokenReader reader)
        {
            reader.Skip(TokenKind.Space);

            return reader.Take().Kind == TokenKind.None;
        }

        /// <summary>
        /// Try to make a reverse path.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <param name="mailbox">The mailbox that was made.</param>
        /// <returns>true if the reverse path was made, false if not.</returns>
        /// <remarks><![CDATA[Path / "<>"]]></remarks>
        public bool TryMakeReversePath(ref TokenReader reader, out IMailbox mailbox)
        {
            if (reader.TryMake(TryMakePath, out mailbox))
            {
                return true;
            }

            if (TryMakeEmptyPath(ref reader))
            {
                mailbox = Mailbox.Empty;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Try to make an empty path.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <returns>true if the empty path was made, false if not.</returns>
        /// <remarks><![CDATA["<>"]]></remarks>
        public bool TryMakeEmptyPath(ref TokenReader reader)
        {
            if (reader.Take().Kind != TokenKind.LessThan)
            {
                return false;
            }

            // not valid according to the spec but some senders do it
            reader.Skip(TokenKind.Space);

            return reader.Take().Kind == TokenKind.GreaterThan;
        }

        /// <summary>
        /// Try to make a path.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <param name="mailbox">The mailbox that was made.</param>
        /// <returns>true if the path was made, false if not.</returns>
        /// <remarks><![CDATA["<" [ A-d-l ":" ] Mailbox ">"]]></remarks>
        public bool TryMakePath(ref TokenReader reader, out IMailbox mailbox)
        {
            mailbox = null;

            if (reader.Take().Kind != TokenKind.LessThan)
            {
                return false;
            }

            // Note, the at-domain-list must be matched, but also must be ignored
            // http://tools.ietf.org/html/rfc5321#appendix-C
            if (reader.TryMake(TryMakeAtDomainList))
            {
                // if the @domain list was matched then it needs to be followed by a colon
                if (reader.Take().Kind != TokenKind.Colon)
                {
                    return false;
                }
            }

            if (TryMakeMailbox(ref reader, out mailbox) == false)
            {
                return false;
            }

            return reader.Take().Kind == TokenKind.GreaterThan;
        }

        /// <summary>
        /// Try to make an @domain list.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <returns>true if the @domain list was made, false if not.</returns>
        /// <remarks><![CDATA[At-domain *( "," At-domain )]]></remarks>
        public bool TryMakeAtDomainList(ref TokenReader reader)
        {
            if (TryMakeAtDomain(ref reader) == false)
            {
                return false;
            }

            while (reader.Peek().Kind == TokenKind.Comma)
            {
                reader.Take();

                if (TryMakeAtDomain(ref reader) == false)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Try to make an @domain.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <returns>true if the @domain was made, false if not.</returns>
        /// <remarks><![CDATA["@" Domain]]></remarks>
        public bool TryMakeAtDomain(ref TokenReader reader)
        {
            if (reader.Take().Kind != TokenKind.At)
            {
                return false;
            }

            return TryMakeDomain(ref reader);
        }

        /// <summary>
        /// Try to make a mailbox.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <param name="mailbox">The mailbox that was made.</param>
        /// <returns>true if the mailbox was made, false if not.</returns>
        /// <remarks><![CDATA[Local-part "@" ( Domain / address-literal )]]></remarks>
        public bool TryMakeMailbox(ref TokenReader reader, out IMailbox mailbox)
        {
            mailbox = null;

            if (reader.TryMake(TryMakeLocalPart, out var localpart) == false)
            {
                return false;
            }

            if (reader.Take().Kind != TokenKind.At)
            {
                return false;
            }

            if (reader.TryMake(TryMakeDomain, out var domain))
            {
                mailbox = CreateMailbox(localpart, domain);
                return true;
            }

            if (reader.TryMake(TryMakeAddressLiteral, out var address))
            {
                mailbox = CreateMailbox(localpart, address);
                return true;
            }

            return false;

            static Mailbox CreateMailbox(ReadOnlySequence<byte> localpart, ReadOnlySequence<byte> domainOrAddress)
            {
                var user = Regex.Unescape(StringUtil.Create(localpart, Encoding.UTF8).Trim('"'));
                
                return new Mailbox(user, StringUtil.Create(domainOrAddress));
            }
        }

        /// <summary>
        /// Try to make a domain name.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <returns>true if the domain name was made, false if not.</returns>
        /// <remarks><![CDATA[sub-domain *("." sub-domain)]]></remarks>
        public bool TryMakeDomain(ref TokenReader reader)
        {
            if (TryMakeSubdomain(ref reader) == false)
            {
                return false;
            }

            while (reader.Peek().Kind == TokenKind.Period)
            {
                reader.Take();

                if (TryMakeSubdomain(ref reader) == false)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Try to make a subdomain name.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <returns>true if the subdomain name was made, false if not.</returns>
        /// <remarks><![CDATA[Let-dig [Ldh-str]]]></remarks>
        public bool TryMakeSubdomain(ref TokenReader reader)
        {
            if (TryMakeTextOrNumber(ref reader) == false)
            {
                return false;
            }

            // this is optional
            reader.TryMake(TryMakeTextOrNumberOrHyphenString);

            return true;
        }

        /// <summary>
        /// Try to make a address.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <returns>true if the address was made, false if not.</returns>
        /// <remarks><![CDATA["[" ( IPv4-address-literal / IPv6-address-literal / General-address-literal ) "]"]]></remarks>
        public bool TryMakeAddressLiteral(ref TokenReader reader)
        {
            if (reader.Take().Kind != TokenKind.LeftBracket)
            {
                return false;
            }

            reader.Skip(TokenKind.Space);

            if (reader.TryMake(TryMakeIPv4AddressLiteral) == false && reader.TryMake(TryMakeIPv6AddressLiteral) == false)
            {
                return false;
            }

            reader.Skip(TokenKind.Space);

            return reader.Take().Kind == TokenKind.RightBracket;
        }

        /// <summary>
        /// Try to make an IPv4 address literal.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <returns>true if the address was made, false if not.</returns>
        /// <remarks><![CDATA[ Snum 3("."  Snum) ]]></remarks>
        public bool TryMakeIPv4AddressLiteral(ref TokenReader reader)
        {
            if (reader.TryMake(TryMakeSnum) == false)
            {
                return false;
            }

            for (var i = 0; i < 3; i++)
            {
                if (reader.Take().Kind != TokenKind.Period)
                {
                    return false;
                }

                if (reader.TryMake(TryMakeSnum) == false)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Try to make an Snum (number in the range of 0-255).
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <returns>true if the snum was made, false if not.</returns>
        /// <remarks><![CDATA[ 1*3DIGIT ]]></remarks>
        public bool TryMakeSnum(ref TokenReader reader)
        {
            if (reader.TryMake(TryMakeNumber, out var number) == false)
            {
                return false;
            }

            return int.TryParse(StringUtil.Create(number), out var snum) && snum >= 0 && snum <= 255;
        }

        /// <summary>
        /// Try to make a Wnum (number in the range of 0-65535).
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <returns>true if the snum was made, false if not.</returns>
        /// <remarks><![CDATA[ 1*5DIGIT ]]></remarks>
        public bool TryMakeWnum(ref TokenReader reader)
        {
            if (reader.TryMake(TryMakeNumber, out var number) == false)
            {
                return false;
            }

            return int.TryParse(StringUtil.Create(number), out var wnum) && wnum >= 0 && wnum <= 65535;
        }

        /// <summary>
        /// Try to extract IPv6 address. https://tools.ietf.org/html/rfc4291 section 2.2 used for specification.
        /// This method expects the address to have the IPv6: prefix.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <returns>true if a valid Ipv6 address can be extracted.</returns>
        public bool TryMakeIPv6AddressLiteral(ref TokenReader reader)
        {
            if (TryMakeIPv6(ref reader) == false)
            {
                return false;
            }

            if (reader.Take().Kind != TokenKind.Colon)
            {
                return false;
            }

            return TryMakeIPv6Address(ref reader);
        }

        /// <summary>
        /// Try to make Ip version from ip version tag which is a formatted text IPv[Version]:
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <returns>true if ip version tag can be extracted.</returns>
        public bool TryMakeIPv6(ref TokenReader reader)
        {
            if (TryMakeIPv(ref reader) == false)
            {
                return false;
            }

            var token = reader.Take();

            if (token.Kind != TokenKind.Number || token.Text.Length > 1)
            {
                return false;
            }

            return token.Text[0] == '6';
        }

        /// <summary>
        /// Try to make the IPv text sequence.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <returns>true if IPv text sequence can be made.</returns>
        public bool TryMakeIPv(ref TokenReader reader)
        {
            if (reader.TryMake(TryMakeText, out var text))
            {
                Span<char> command = stackalloc char[3];
                command[0] = 'I';
                command[1] = 'P';
                command[2] = 'v';

                return text.CaseInsensitiveStringEquals(ref command);
            }

            return false;
        }

        /// <summary>
        /// Try to make an IPv6 address.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <returns>true if the address was made, false if not.</returns>
        /// <remarks><![CDATA[  ]]></remarks>
        public bool TryMakeIPv6Address(ref TokenReader reader)
        {
            return reader.TryMake(TryMakeIPv6AddressRule1)
                || reader.TryMake(TryMakeIPv6AddressRule2)
                || reader.TryMake(TryMakeIPv6AddressRule3)
                || reader.TryMake(TryMakeIPv6AddressRule4)
                || reader.TryMake(TryMakeIPv6AddressRule5)
                || reader.TryMake(TryMakeIPv6AddressRule6)
                || reader.TryMake(TryMakeIPv6AddressRule7)
                || reader.TryMake(TryMakeIPv6AddressRule8)
                || reader.TryMake(TryMakeIPv6AddressRule9);
        }

        bool TryMakeIPv6AddressRule1(ref TokenReader reader)
        {
            // 6( h16 ":" ) ls32
            return TryMakeIPv6HexPostamble(ref reader, 6);
        }

        bool TryMakeIPv6AddressRule2(ref TokenReader reader)
        {
            // "::" 5( h16 ":" ) ls32
            if (reader.Take().Kind != TokenKind.Colon || reader.Take().Kind != TokenKind.Colon)
            {
                return false;
            }

            return TryMakeIPv6HexPostamble(ref reader, 5);
        }

        bool TryMakeIPv6AddressRule3(ref TokenReader reader)
        {
            // [ h16 ] "::" 4( h16 ":" ) ls32
            if (TryMakeIPv6HexPreamble(ref reader, 1) == false)
            {
                return false;
            }

            return TryMakeIPv6HexPostamble(ref reader, 4);
        }

        bool TryMakeIPv6AddressRule4(ref TokenReader reader)
        {
            // [ *1( h16 ":" ) h16 ] "::" 3( h16 ":" ) ls32
            if (TryMakeIPv6HexPreamble(ref reader, 2) == false)
            {
                return false;
            }

            return TryMakeIPv6HexPostamble(ref reader, 3);
        }

        bool TryMakeIPv6AddressRule5(ref TokenReader reader)
        {
            // [ *2( h16 ":" ) h16 ] "::" 2( h16 ":" ) ls32
            if (TryMakeIPv6HexPreamble(ref reader, 3) == false)
            {
                return false;
            }

            return TryMakeIPv6HexPostamble(ref reader, 2);
        }

        bool TryMakeIPv6AddressRule6(ref TokenReader reader)
        {
            // [ *3( h16 ":" ) h16 ] "::" h16 ":" ls32
            if (TryMakeIPv6HexPreamble(ref reader, 4) == false)
            {
                return false;
            }

            return TryMakeIPv6HexPostamble(ref reader, 1);
        }

        bool TryMakeIPv6AddressRule7(ref TokenReader reader)
        {
            // [ *4( h16 ":" ) h16 ] "::" ls32
            if (TryMakeIPv6HexPreamble(ref reader, 5) == false)
            {
                return false;
            }

            return TryMakeIPv6Ls32(ref reader);
        }

        bool TryMakeIPv6AddressRule8(ref TokenReader reader)
        {
            // [ *5( h16 ":" ) h16 ] "::" h16
            if (TryMakeIPv6HexPreamble(ref reader, 6) == false)
            {
                return false;
            }

            return TryMake16BitHex(ref reader);
        }

        bool TryMakeIPv6AddressRule9(ref TokenReader reader)
        {
            // [ *6( h16 ":" ) h16 ] "::"
            return TryMakeIPv6HexPreamble(ref reader, 7);
        }

        bool TryMakeIPv6HexPreamble(ref TokenReader reader, int maximum)
        {
            for (var i = 0; i < maximum; i++)
            {
                if (reader.TryMake(TryMakeTerminal))
                {
                    return true;
                }

                if (i > 0)
                {
                    if (reader.Take().Kind != TokenKind.Colon)
                    {
                        return false;
                    }
                }

                if (TryMake16BitHex(ref reader) == false)
                {
                    return false;
                }
            }

            return reader.TryMake(TryMakeTerminal);

            static bool TryMakeTerminal(ref TokenReader reader)
            {
                return reader.Take().Kind == TokenKind.Colon && reader.Take().Kind == TokenKind.Colon;
            }
        }

        bool TryMakeIPv6HexPostamble(ref TokenReader reader, int count)
        {
            while (count-- > 0)
            {
                if (TryMake16BitHex(ref reader) == false)
                {
                    return false;
                }

                if (reader.Take().Kind != TokenKind.Colon)
                {
                    return false;
                }
            }

            return TryMakeIPv6Ls32(ref reader);
        }

        bool TryMakeIPv6Ls32(ref TokenReader reader)
        {
            if (reader.TryMake(TryMakeIPv4AddressLiteral))
            {
                return true;
            }

            if (TryMake16BitHex(ref reader) == false)
            {
                return false;
            }

            if (reader.Take().Kind != TokenKind.Colon)
            {
                return false;
            }

            return TryMake16BitHex(ref reader);
        }

        /// <summary>
        /// Try to make 16 bit hex number.
        /// </summary>
        /// <param name="reader">The token reader to perform the operation on.</param>
        /// <returns>true if valid hex number can be extracted.</returns>
        public bool TryMake16BitHex(ref TokenReader reader)
        {
            var hexLength = 0L;

            var token = reader.Peek();
            while ((token.Kind == TokenKind.Text || token.Kind == TokenKind.Number) && hexLength < 4)
            {
                if (token.Kind == TokenKind.Text && IsHex(ref token) == false)
                {
                    return false;
                }

                hexLength += reader.Take().Text.Length;

                token = reader.Peek();
            }

            return hexLength > 0 && hexLength <= 4;

            static bool IsHex(ref Token token)
            {
                var span = token.Text;

                return span.IsHex();
            }
        }

        /// <summary>
        /// Try to make a text/number/hyphen string.
        /// </summary>
        /// <param name="reader">The reader to perform the operatio on.</param>
        /// <returns>true if a text, number or hyphen was made, false if not.</returns>
        /// <remarks><![CDATA[*( ALPHA / DIGIT / "-" ) Let-dig]]></remarks>
        public bool TryMakeTextOrNumberOrHyphenString(ref TokenReader reader)
        {
            var token = reader.Peek();

            if (token.Kind == TokenKind.Text || token.Kind == TokenKind.Number || token.Kind == TokenKind.Hyphen)
            {
                reader.Skip(kind => kind == TokenKind.Text || kind == TokenKind.Number || kind == TokenKind.Hyphen);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Try to make a text or number
        /// </summary>
        /// <param name="reader">The reader to perform the operatio on.</param>
        /// <returns>true if the text or number was made, false if not.</returns>
        /// <remarks><![CDATA[ALPHA / DIGIT]]></remarks>
        public bool TryMakeTextOrNumber(ref TokenReader reader)
        {
            var token = reader.Peek();

            if (token.Kind == TokenKind.Text)
            {
                return TryMakeText(ref reader);
            }

            if (token.Kind == TokenKind.Number)
            {
                return TryMakeNumber(ref reader);
            }

            return false;
        }

        /// <summary>
        /// Try to make the local part of the path.
        /// </summary>
        /// <param name="reader">The reader to perform the operatio on.</param>
        /// <returns>true if the local part was made, false if not.</returns>
        /// <remarks><![CDATA[Dot-string / Quoted-string]]></remarks>
        public bool TryMakeLocalPart(ref TokenReader reader)
        {
            if (reader.TryMake(TryMakeDotString))
            {
                return true;
            }

            return TryMakeQuotedString(ref reader);
        }

        /// <summary>
        /// Try to make a dot-string from the tokens.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <returns>true if the dot-string was made, false if not.</returns>
        /// <remarks><![CDATA[Atom *("."  Atom)]]></remarks>
        public bool TryMakeDotString(ref TokenReader reader)
        {
            if (TryMakeAtom(ref reader) == false)
            {
                return false;
            }

            while (reader.Peek().Kind == TokenKind.Period)
            {
                reader.Take();

                if (TryMakeAtom(ref reader) == false)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Try to make a quoted-string from the tokens.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <returns>true if the quoted-string was made, false if not.</returns>
        /// <remarks><![CDATA[DQUOTE * QcontentSMTP DQUOTE]]></remarks>
        public bool TryMakeQuotedString(ref TokenReader reader)
        {
            if (reader.Take().Kind != TokenKind.Quote)
            {
                return false;
            }

            while (reader.Peek().Kind != TokenKind.Quote)
            {
                if (TryMakeQContentSmtp(ref reader) == false)
                {
                    return false;
                }
            }

            return reader.Take().Kind == TokenKind.Quote;
        }

        /// <summary>
        /// Try to make a QcontentSMTP from the tokens.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <returns>true if the quoted content was made, false if not.</returns>
        /// <remarks><![CDATA[qtextSMTP / quoted-pairSMTP]]></remarks>
        public bool TryMakeQContentSmtp(ref TokenReader reader)
        {
            if (reader.TryMake(TryMakeQTextSmtp))
            {
                return true;
            }

            return TryMakeQuotedPairSmtp(ref reader);
        }

        /// <summary>
        /// Try to make a QTextSMTP from the tokens.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <returns>true if the quoted text was made, false if not.</returns>
        /// <remarks><![CDATA[%d32-33 / %d35-91 / %d93-126]]></remarks>
        public bool TryMakeQTextSmtp(ref TokenReader reader)
        {
            if (reader.Peek().Kind == TokenKind.None)
            {
                return false;
            }

            switch (reader.Peek().Kind)
            {
                case TokenKind.Text:
                    return TryMakeText(ref reader);

                case TokenKind.Number:
                    return TryMakeNumber(ref reader);

                default:
                    var token = reader.Take();
                    switch ((char)token.Text[0])
                    {
                        case ' ':
                        case '!':
                        case '#':
                        case '$':
                        case '%':
                        case '&':
                        case '\'':
                        case '(':
                        case ')':
                        case '*':
                        case '+':
                        case ',':
                        case '-':
                        case '.':
                        case '/':
                        case ':':
                        case ';':
                        case '<':
                        case '=':
                        case '>':
                        case '?':
                        case '@':
                        case '[':
                        case ']':
                        case '^':
                        case '_':
                        case '`':
                        case '{':
                        case '|':
                        case '}':
                        case '~':
                            return true;
                    }
                    break;
            }

            return false;
        }

        /// <summary>
        /// Try to make a quoted pair from the tokens.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <returns>true if the quoted pair was made, false if not.</returns>
        /// <remarks><![CDATA[%d92 %d32-126]]></remarks>
        public bool TryMakeQuotedPairSmtp(ref TokenReader reader)
        {
            if (reader.Take().Kind != TokenKind.Backslash)
            {
                return false;
            }

            var token = reader.Take();

            return token.Text.Length > 0 && token.Text[0] >= 32 && token.Text[0] <= 126;
        }

        /// <summary>
        /// Try to make an "Atom" from the tokens.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <returns>true if the atom was made, false if not.</returns>
        /// <remarks><![CDATA[1*atext]]></remarks>
        public bool TryMakeAtom(ref TokenReader reader)
        {
            var count = 0;

            while (reader.TryMake(TryMakeAtext))
            {
                count++;
            }

            return count >= 1;
        }

        /// <summary>
        /// Try to make an "Atext" from the tokens.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <returns>true if the atext was made, false if not.</returns>
        /// <remarks><![CDATA[atext]]></remarks>
        public bool TryMakeAtext(ref TokenReader reader)
        {
            var peek = reader.Peek();

            switch (peek.Kind)
            {
                case TokenKind.None:
                    return false;

                case TokenKind.Text:
                    return TryMakeText(ref reader);

                case TokenKind.Number:
                    return TryMakeNumber(ref reader);

                default:
                    var token = reader.Take();
                    switch ((char)token.Text[0])
                    {
                        case '!':
                        case '#':
                        case '%':
                        case '&':
                        case '\'':
                        case '*':
                        case '-':
                        case '/':
                        case '?':
                        case '_':
                        case '{':
                        case '}':
                        case '$':
                        case '+':
                        case '=':
                        case '^':
                        case '`':
                        case '|':
                        case '~':
                            return true;
                    }
                    break;
            }

            return false;
        }

        /// <summary>
        /// Try to make an Mail-Parameters from the tokens.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <param name="parameters">The mail parameters that were made.</param>
        /// <returns>true if the mail parameters can be made, false if not.</returns>
        /// <remarks><![CDATA[esmtp-param *(SP esmtp-param)]]></remarks>
        public bool TryMakeMailParameters(ref TokenReader reader, out IReadOnlyDictionary<string, string> parameters)
        {
            Dictionary<string, string> dictionary = null;

            while (reader.Peek().Kind != TokenKind.None)
            {
                if (reader.TryMake(TryMakeEsmtpParameter, out ReadOnlySequence<byte> keyword, out ReadOnlySequence<byte> value) == false)
                {
                    parameters = null;
                    return false;
                }

                dictionary ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                dictionary.Add(StringUtil.Create(keyword), StringUtil.Create(value));

                reader.Skip(TokenKind.Space);
            }

            parameters = dictionary;
            return parameters?.Count > 0;
        }

        /// <summary>
        /// Try to make an Esmtp-Parameter from the tokens.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <param name="keyword">The keyword that was made.</param>
        /// <param name="value">The value that was made.</param>
        /// <returns>true if the esmtp-parameter can be made, false if not.</returns>
        /// <remarks><![CDATA[esmtp-keyword ["=" esmtp-value]]]></remarks>
        public bool TryMakeEsmtpParameter(ref TokenReader reader, out ReadOnlySequence<byte> keyword, out ReadOnlySequence<byte> value)
        {
            value = default;

            if (reader.TryMake(TryMakeEsmtpKeyword, out keyword) == false)
            {
                return false;
            }

            if (reader.Peek().Kind == TokenKind.None || reader.Peek().Kind == TokenKind.Space)
            {
                return true;
            }

            if (reader.Take().Kind != TokenKind.Equal)
            {
                return false;
            }

            return reader.TryMake(TryMakeEsmtpValue, out value);
        }

        /// <summary>
        /// Try to make an Esmtp-Keyword from the tokens.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <returns>true if the esmtp-keyword can be made, false if not.</returns>
        /// <remarks><![CDATA[(ALPHA / DIGIT) *(ALPHA / DIGIT / "-")]]></remarks>
        public bool TryMakeEsmtpKeyword(ref TokenReader reader)
        {
            var token = reader.Take();
            if (token.Kind != TokenKind.Text && token.Kind != TokenKind.Number)
            {
                return false;
            }

            token = reader.Peek();
            while (token.Kind == TokenKind.Text || token.Kind == TokenKind.Number || token.Kind == TokenKind.Hyphen)
            {
                reader.Take();
                token = reader.Peek();
            }

            return true;
        }

        /// <summary>
        /// Try to make an Esmtp-Value from the tokens.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <returns>true if the esmtp-value can be made, false if not.</returns>
        /// <remarks><![CDATA[1*(%d33-60 / %d62-127)]]></remarks>
        public bool TryMakeEsmtpValue(ref TokenReader reader)
        {
            var token = reader.Take();
            if (token.Kind == TokenKind.None || IsValid(ref token) == false)
            {
                return false;
            }

            token = reader.Peek();
            while (token.Kind != TokenKind.None && IsValid(ref token))
            {
                reader.Take();

                token = reader.Peek();
            }

            return true;

            static bool IsValid(ref Token token)
            {
                var span = token.Text;

                for (var i = 0; i < span.Length; i++)
                {
                    if ((span[i] < 33 || span[i] > 60) && (span[i] < 62 || span[i] > 127))
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        /// <summary>
        /// Try to make a base64 encoded string.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <returns>true if the base64 encoded string can be made, false if not.</returns>
        /// <remarks><![CDATA[ALPHA / DIGIT / "+" / "/"]]></remarks>
        public bool TryMakeBase64(ref TokenReader reader)
        {
            if (TryMakeBase64Text(ref reader) == false)
            {
                return false;
            }

            if (reader.Peek().Kind == TokenKind.Equal)
            {
                reader.Take();
            }

            if (reader.Peek().Kind == TokenKind.Equal)
            {
                reader.Take();
            }
            
            return true;
        }

        /// <summary>
        /// Try to make a base64 encoded string.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <returns>true if the base64 encoded string can be made, false if not.</returns>
        /// <remarks><![CDATA[ALPHA / DIGIT / "+" / "/"]]></remarks>
        public bool TryMakeBase64Text(ref TokenReader reader)
        {
            var count = 0;

            while (reader.TryMake(TryMakeBase64Chars))
            {
                count++;
            }

            return count > 0;
        }

        /// <summary>
        /// Try to make the allowable characters in a base64 encoded string.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <returns>true if the base64-chars can be made, false if not.</returns>
        /// <remarks><![CDATA[ALPHA / DIGIT / "+" / "/"]]></remarks>
        public bool TryMakeBase64Chars(ref TokenReader reader)
        {
            var token = reader.Take();
            
            switch (token.Kind)
            {
                case TokenKind.Text:
                case TokenKind.Number:
                case TokenKind.Slash:
                case TokenKind.Plus:
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Try to make a text sequence.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <returns>true if a text sequence could be made, false if not.</returns>
        public bool TryMakeText(ref TokenReader reader)
        {
            if (reader.Peek().Kind == TokenKind.Text)
            {
                reader.Skip(TokenKind.Text);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Try to make a number sequence.
        /// </summary>
        /// <param name="reader">The reader to perform the operation on.</param>
        /// <returns>true if a number sequence could be made, false if not.</returns>
        public bool TryMakeNumber(ref TokenReader reader)
        {
            if (reader.Peek().Kind == TokenKind.Number)
            {
                reader.Skip(TokenKind.Number);
                return true;
            }

            return false;
        }



        public bool TryMakeSomethingText(ref TokenReader reader)
        {
          var count = 0;

          while (reader.TryMake(TryMakeSomethingChars))
          {
            count++;
          }

          return count > 0;
        }

        public bool TryMakeSomethingChars(ref TokenReader reader)
        {
          var token = reader.Take();

          switch (token.Kind)
          {
            case TokenKind.Text:
            case TokenKind.Number:
            case TokenKind.Space:
            case TokenKind.Hyphen:
            case TokenKind.Period:
            case TokenKind.LeftBracket:
            case TokenKind.RightBracket:
            case TokenKind.Colon:
            case TokenKind.GreaterThan:
            case TokenKind.LessThan:
            case TokenKind.Comma:
            case TokenKind.At:
            case TokenKind.Quote:
            case TokenKind.Equal:
            case TokenKind.Slash:
            case TokenKind.Backslash:
            case TokenKind.Plus:
              return true;
          }

          return false;
        }


  }
}
