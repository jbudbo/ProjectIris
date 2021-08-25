using StackExchange.Redis;

namespace Microsoft.Extensions.Logging
{
    internal static class LoggerExtensions
    {
        private static readonly Action<ILogger, Exception> startup;
        private static readonly Action<ILogger, Exception> loadEmoji;
        private static readonly Action<ILogger, int, Exception> emojisLoaded;
        private static readonly Action<ILogger, Exception> shutdown;
        private static readonly Action<ILogger, RedisValue, Exception> tweetReceived;
        private static readonly Action<ILogger, Uri, string, Exception> downloadingEmojiData;

        static LoggerExtensions()
        {
            startup = LoggerMessage.Define(LogLevel.Information, new(1, nameof(Startup)), "Tweet worker coming up");
            shutdown = LoggerMessage.Define(LogLevel.Information, new(2, nameof(Shutdown)), "Tweet worker is shutting down");

            loadEmoji = LoggerMessage.Define(LogLevel.Information, new(3, nameof(LoadEmojiData)), "Loading Emoji Data");
            emojisLoaded = LoggerMessage.Define<int>(LogLevel.Information, new(4, nameof(EmojisLoaded)), "{count} Emojis Loaded");
            tweetReceived = LoggerMessage.Define<RedisValue>(LogLevel.Information, new (5, nameof(TweetReceived)), "Tweet Received: {tweet}");
            downloadingEmojiData = LoggerMessage.Define<Uri, string>(LogLevel.Information, new(6, nameof(DownloadingEmojiData)), "Downloading Emoji data from {baseUri}{resource}");
        }

        internal static void Startup(this ILogger logger) => startup(logger, null);
        internal static void Shutdown(this ILogger logger) => shutdown(logger, null);

        internal static void LoadEmojiData(this ILogger logger) => loadEmoji(logger, null);
        internal static void EmojisLoaded(this ILogger logger, int count) => emojisLoaded(logger, count, null);

        internal static void TweetReceived(this ILogger logger, RedisValue tweet) => tweetReceived(logger, tweet.ToString(), null);

        /// <summary>
        /// Logs an Informational message about where Emoji data is being downloaded from
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="baseUri"></param>
        /// <param name="resource"></param>
        internal static void DownloadingEmojiData(this ILogger logger, Uri baseUri, string resource) => downloadingEmojiData(logger, baseUri, resource, null);

    }
}
