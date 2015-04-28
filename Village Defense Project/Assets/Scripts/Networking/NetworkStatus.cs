namespace Gem
{
    namespace Networking
    {
        public static class NetworkStatus
        {
            public const int ERROR = -1;
            public const int BAD = 0;
            public const int GOOD = 1;
            public const int FULL = 2;
            public const int INVALID_USERNAME = 3;
            public const int INVALID_PASSWORD = 4;
            public const int ACCOUNT_ALREADY_EXISTS = 5;
            public const int TIMED_OUT = 6;
        }
    }
}