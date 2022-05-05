using StackExchange.Redis;
using System;

namespace Microsoft.Extensions.Logging
{
    internal static partial class LoggerExtensions
    {
        private static readonly Action<ILogger, Exception> loadEmoji;
        private static readonly Action<ILogger, int, Exception> emojisLoaded;
        private static readonly Action<ILogger, RedisValue, Exception> tweetReceived;
        private static readonly Action<ILogger, Uri, string, Exception> downloadingEmojiData;

        static LoggerExtensions()
        {
            loadEmoji = LoggerMessage.Define(LogLevel.Information, new(3, nameof(EmojisLoading)), "Loading Emoji Data");
            emojisLoaded = LoggerMessage.Define<int>(LogLevel.Information, new(4, nameof(EmojisLoaded)), "{count} Emojis Loaded");
            tweetReceived = LoggerMessage.Define<RedisValue>(LogLevel.Information, new (5, nameof(TweetReceived)), "Tweet Received: {tweet}");
            downloadingEmojiData = LoggerMessage.Define<Uri, string>(LogLevel.Information, new(6, nameof(DownloadingEmojiData)), "Downloading Emoji data from {baseUri}{resource}");
        }

        /// <summary>
        /// Logs an Emoji Data Loading event
        /// </summary>
        /// <param name="logger"></param>
        internal static void EmojisLoading(this ILogger logger) => loadEmoji(logger, null);

        /// <summary>
        /// Logs an Emoji Data Loaded event
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="count"></param>
        internal static void EmojisLoaded(this ILogger logger, int count) => emojisLoaded(logger, count, null);

        /// <summary>
        /// Logs a Tweet Received event
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="tweet"></param>
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
