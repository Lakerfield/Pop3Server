namespace Pop3Server
{
    public sealed class AuthenticationContext
    {
        public static readonly AuthenticationContext Unauthenticated = new AuthenticationContext();

        /// <summary>
        /// Constructor.
        /// </summary>
        public AuthenticationContext()
        {
            IsAuthenticated = false;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="isAuthenticated">whether or nor the current session is authenticated.</param>
        /// <param name="user">The name of the user that is trying or was authenticated.</param>
        public AuthenticationContext(bool isAuthenticated, string user)
        {
            User = user;
            IsAuthenticated = isAuthenticated;
        }

        /// <summary>
        /// The name of the user that is trying or was authenticated.
        /// </summary>
        public string? User { get; }

        /// <summary>
        /// Returns a value indicating whether or nor the current session is authenticated.
        /// </summary>
        public bool IsAuthenticated { get; }
    }
}
