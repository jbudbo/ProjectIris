using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text;
using System.Text.Json;

namespace Worker
{
    using Interfaces;
    using Models;
    using System;

    internal sealed class TweetWorker : IHostedService
    {
        private readonly ILogger<TweetWorker> logger;
        private readonly IEmojiClient client;
        private readonly IConnectionMultiplexer redis;

        private EmojiData[] emojiCache;

        public TweetWorker(
            ILogger<TweetWorker> logger,
            IEmojiClient client,
            IConnectionMultiplexer redis)
        {
            this.logger = logger;
            this.client = client;
            this.redis = redis;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            logger.Startup();

            await LoadEmojiDataAsync(cancellationToken);

            IDatabase db = redis.GetDatabase();

            while (!cancellationToken.IsCancellationRequested)
            {
                RedisValue rawTweet = await db.ListLeftPopAsync("tweets");

                if (!rawTweet.HasValue)
                    continue;

                logger.TweetReceived(rawTweet);

                using MemoryStream buff = new(Encoding.UTF8.GetBytes(rawTweet));

                Tweet tweet = await JsonSerializer.DeserializeAsync<Tweet>(buff, cancellationToken: cancellationToken);

            }
        }

        private async Task LoadEmojiDataAsync(CancellationToken cancellationToken)
        {
            logger.LoadEmojiData();

            emojiCache = await client.DownloadEmojisAsync(cancellationToken);

            logger.EmojisLoaded(emojiCache.Length);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.Shutdown();

            return Task.CompletedTask;
        }
    }
}