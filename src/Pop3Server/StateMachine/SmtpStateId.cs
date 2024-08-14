namespace Pop3Server.StateMachine
{
    internal enum SmtpStateId
    {
        None = 0,
        Authorization = 1,
        AuthorizationSecure = 2,
        Transaction = 3,
        Update = 4,


        //Initialized = 1,
        //WaitingForMail = 2,
        //WaitingForMailSecure = 3,
        //WithinTransaction = 4,
        //CanAcceptData = 5,
    }
}
