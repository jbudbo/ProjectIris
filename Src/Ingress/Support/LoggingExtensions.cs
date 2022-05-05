using System;

namespace Microsoft.Extensions.Logging
{
    internal static partial class LoggerExtensions
    {
        private static readonly Action<ILogger, Uri, Exception> connecting;
        private static readonly Action<ILogger, string, Exception> tweetReceived;

        static LoggerExtensions()
        {
            connecting = LoggerMessage.Define<Uri>(LogLevel.Information, new(4, nameof(Connecting)), "Connecting to {uri}");
            tweetReceived = LoggerMessage.Define<string>(LogLevel.Information, new EventId(3, nameof(TweetReceived)), "Tweeting: {tweet}");
        }

        /// <summary>
        /// Logs a Twitter Connecting event
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="uri"></param>
        internal static void Connecting(this ILogger logger, Uri uri) => connecting(logger, uri, null);

        /// <summary>
        /// Logs a Tweet Received event
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="tweet"></param>
        internal static void TweetReceived(this ILogger logger, string tweet) => tweetReceived(logger, tweet.ToString(), null);
    }
}
