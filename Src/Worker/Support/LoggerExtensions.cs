using StackExchange.Redis;

namespace Microsoft.Extensions.Logging
{
    internal static class LoggerExtensions
    {
        private static readonly Action<ILogger, Exception> _startup;
        private static readonly Action<ILogger, Exception> _shutdown;
        private static readonly Action<ILogger, RedisValue, Exception> _tweetReceived;

        static LoggerExtensions()
        {
            _startup = LoggerMessage.Define(LogLevel.Information, new(1, nameof(Startup)), "Tweet worker coming up");
            _shutdown = LoggerMessage.Define(LogLevel.Information, new(2, nameof(Shutdown)), "Tweet worker is shutting down");
            _tweetReceived = LoggerMessage.Define<RedisValue>(LogLevel.Information, new EventId(3, nameof(TweetReceived)), "Tweet Received: {tweet}");
        }

        internal static void Startup(this ILogger logger) => _startup(logger, null);
        internal static void Shutdown(this ILogger logger) => _shutdown(logger, null);
        internal static void TweetReceived(this ILogger logger, RedisValue tweet) => _tweetReceived(logger, tweet.ToString(), null);

    }
}
