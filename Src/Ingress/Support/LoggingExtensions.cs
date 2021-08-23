namespace Microsoft.Extensions.Logging
{
    internal static class LoggerExtensions
    {
        private static readonly Action<ILogger, Uri, Exception> _connecting;
        private static readonly Action<ILogger, Exception> _startup;
        private static readonly Action<ILogger, Exception> _shutdown;
        private static readonly Action<ILogger, string, Exception> _tweetReceived;

        static LoggerExtensions()
        {
            _connecting = LoggerMessage.Define<Uri>(LogLevel.Information, new(4, nameof(Connecting)), "Connecting to {uri}");

            _startup = LoggerMessage.Define(LogLevel.Information, new(1, nameof(Startup)), "Ingress worker coming up");
            _shutdown = LoggerMessage.Define(LogLevel.Information, new(2, nameof(Shutdown)), "Ingress worker is shutting down");
            _tweetReceived = LoggerMessage.Define<string>(LogLevel.Information, new EventId(3, nameof(TweetReceived)), "Tweeting: {tweet}");
        }

        internal static void Connecting(this ILogger logger, Uri uri) => _connecting(logger, uri, null);
        internal static void Startup(this ILogger logger) => _startup(logger, null);
        internal static void Shutdown(this ILogger logger) => _shutdown(logger, null);
        internal static void TweetReceived(this ILogger logger, string tweet) => _tweetReceived(logger, tweet.ToString(), null);
    }
}
