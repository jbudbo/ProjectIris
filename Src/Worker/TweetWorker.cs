using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text;
using System.Text.Json;

namespace Worker
{
    using Interfaces;
    using Models;

    internal sealed class TweetWorker : IHostedService
    {
        private readonly ILogger<TweetWorker> logger;
        private readonly IEmojiClient client;
        private readonly IConnectionMultiplexer redis;

        private EmojiMasterList emojiCache;

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

            await LoadEmojiDataAsync(cancellationToken)
                .ConfigureAwait(false);

            IDatabase db = redis.GetDatabase();

            while (!cancellationToken.IsCancellationRequested)
            {
                RedisValue rawTweet = await db.ListLeftPopAsync("tweets")
                .ConfigureAwait(false);

                if (!rawTweet.HasValue)
                    continue;

                logger.TweetReceived(rawTweet);

                using MemoryStream buff = new(Encoding.UTF8.GetBytes(rawTweet));

                Tweet tweet = await JsonSerializer.DeserializeAsync<Tweet>(buff, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                if (tweet.data is null) //  Something went wrong with our tweet stream
                {
                    logger.LogError("Woops");
                    return;
                }

                var entities = tweet.data.entities;

                ITransaction trans = db.CreateTransaction();

                Task tEmoji = AggEmojiAsync(trans, tweet.data.text);

                await AggHashTagsAsync(trans, entities.hashtags)
                    .ConfigureAwait(false);

                await AggUrlsAsync(trans, entities.urls)
                    .ConfigureAwait(false);

                await AggAnnotationsAsync(trans, entities.annotations)
                    .ConfigureAwait(false);

                await AggMentionsAsync(trans, entities.mentions)
                    .ConfigureAwait(false);
                
                await tEmoji.ConfigureAwait(false);

                await trans.ExecuteAsync(CommandFlags.FireAndForget)
                    .ConfigureAwait(false);
            }
        }

        private async Task AggEmojiAsync(ITransaction trans, string text)
        {
            await trans.StringIncrementAsync("emojiCount", flags: CommandFlags.FireAndForget)
                .ConfigureAwait(false);

            foreach (var emoji in emojiCache.ContainsEmojis(text))
            {
                await trans.HashIncrementAsync("emojis", emoji, flags: CommandFlags.FireAndForget)
                    .ConfigureAwait(false);
            }
        }

        private static async Task AggHashTagsAsync(ITransaction trans, HashtagEntity[] hashtags)
        {
            if (hashtags is null || hashtags.Length is 0)
                return;

            await trans.StringIncrementAsync("hashTagCount", flags: CommandFlags.FireAndForget)
                .ConfigureAwait(false);

            for (int i = 0, j = hashtags.Length; i < j; i++)
            {
                await trans.HashIncrementAsync("hashtags", hashtags[i].tag, flags: CommandFlags.FireAndForget)
                    .ConfigureAwait(false);
            }
        }

        private static async Task AggUrlsAsync(ITransaction trans, UrlEntity[] urls)
        {
            if (urls is null || urls.Length is 0)
                return;

            await trans.StringIncrementAsync("urlCount", flags: CommandFlags.FireAndForget)
                .ConfigureAwait(false);

            for (int i = 0, j = urls.Length; i < j; i++)
            {
                Uri uri = urls[i].expanded_url;

                await trans.HashIncrementAsync("domains", uri.Host, flags: CommandFlags.FireAndForget)
                    .ConfigureAwait(false);
            }
        }

        private static async Task AggAnnotationsAsync(ITransaction trans, AnnotationEntity[] annotations)
        {
            if (annotations is null || annotations.Length is 0)
                return;

            await trans.StringIncrementAsync("annotationCount", flags: CommandFlags.FireAndForget)
                .ConfigureAwait(false);

            for (int i = 0, j = annotations.Length; i < j; i++)
            {
                await trans.HashIncrementAsync("annotations", annotations[i].normalized_text, flags: CommandFlags.FireAndForget)
                    .ConfigureAwait(false);
            }
        }

        private static async Task AggMentionsAsync(ITransaction trans, MentionEntity[] mentions)
        {
            if (mentions is null || mentions.Length is 0)
                return;

            await trans.StringIncrementAsync("mentionCount", flags: CommandFlags.FireAndForget)
                .ConfigureAwait(false);

            for (int i = 0, j = mentions.Length; i < j; i++)
            {
                await trans.HashIncrementAsync("mentions", mentions[i].username, flags: CommandFlags.FireAndForget)
                    .ConfigureAwait(false);
            }
        }

        private async Task LoadEmojiDataAsync(CancellationToken cancellationToken)
        {
            logger.LoadEmojiData();

            emojiCache = await client.DownloadEmojisAsync(cancellationToken)
                .ConfigureAwait(false);

            logger.EmojisLoaded(emojiCache.Count);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.Shutdown();

            return Task.CompletedTask;
        }
    }
}