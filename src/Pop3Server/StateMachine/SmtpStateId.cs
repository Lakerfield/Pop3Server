namespace Pop3Server.StateMachine
{
    internal enum SmtpStateId
    {
        None = 0,
        Authorization = 1,
        AuthorizationSecure = 2,
        AuthorizationWaitForPassword = 3,
        Transaction = 5,
        Update = 6,


        //Initialized = 1,
        //WaitingForMail = 2,
        //WaitingForMailSecure = 3,
        //WithinTransaction = 4,
        //CanAcceptData = 5,
    }
}
