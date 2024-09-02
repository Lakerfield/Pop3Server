# What is Pop3Server?

[![NuGet](https://img.shields.io/nuget/v/Lakerfield.Pop3Server.svg)](https://www.nuget.org/packages/Lakerfield.Pop3Server/)

Pop3Server is a simple, yet highly functional POP3 server implementation. 
It is based on the structure of the [SmtpServer](https://github.com/cosullivan/SmtpServer) project by cosullivan, with modifications to implement POP3 instead of SMTP. 
Written entirely in C#, showcases the potential for a full-featured POP3 server built on .NET.

Pop3Server is available via [NuGet](https://www.nuget.org/packages/Lakerfield.Pop3Server/)

# Acknowledgements

Special thanks to [cosullivan](https://github.com/cosullivan) for the original SmtpServer project, which served as the foundation for Pop3Server.

# What's New?

This project is in its early stages and is a proof of concept for a POP3 server. The code structure and architecture have been adapted from the original SmtpServer project to support POP3.

# How can it be used?

To get the Pop3Server up and running, you can follow this example from the included project in the `example\Pop3WorkerService` folder. Here's how to start the server:

```cs
var options = new Pop3ServerOptionsBuilder()
  .ServerName("POP3 Server")
  .Endpoint(endpoint => endpoint
    .Port(110)
    .AllowUnsecureAuthentication())
  .Build();

var serviceProvider = new ServiceProvider();
serviceProvider.Add(new ConsoleUserAuthenticator());
serviceProvider.Add(new ConsoleMessageStore());

var pop3Server = new Pop3Server.Pop3Server(options, serviceProvider);
await pop3Server.StartAsync(CancellationToken.None);
```

# Available Features

Pop3Server currently supports the following POP3 commands:

- CAPA
- USER
- PASS
- DELE
- LIST
- NOOP
- PROXY
- RETR 
- RSET
- STAT
- STLS
- TOP
- UIDL
- QUIT

# Example Project

An example project demonstrating how to set up and run the Pop3Server is included in the `example\Pop3WorkerService` folder. This example provides a basic implementation to help you get started with using and customizing the server.

# Contributing

Contributions are welcome! If you find bugs or have feature requests, feel free to open an issue or submit a pull request.

# License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for more details.
