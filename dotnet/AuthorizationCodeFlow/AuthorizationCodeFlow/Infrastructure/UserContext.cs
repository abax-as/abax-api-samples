namespace AuthorizationCodeFlow.Infrastructure
{
    public static class UserContext
    {
        // this represents your user currently logged in into your system
        // normally, you would get the user from your authentication system
        // here, for simplicity, we assume that logged in user is "user1"
        public const string User = "user1";
    }
}