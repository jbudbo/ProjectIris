using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text;
using System.Text.Json;

namespace Worker
{
    using Interfaces;
    using Models;
    using Support;

    /// <summary>
    /// A worker process for consuming raw Tweet data and depositing metrics around the data
    /// </summary>
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
            logger.Startup(nameof(TweetWorker));

            try
            {
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

                    await using MemoryStream buff = new(Encoding.UTF8.GetBytes(rawTweet));

                    Tweet tweet = await JsonSerializer.DeserializeAsync<Tweet>(buff, cancellationToken: cancellationToken)
                        .ConfigureAwait(false);

                    if (tweet.data is null)
                    {
                        logger.LogError("Something went wrong with our tweet stream");
                        return;
                    }

                    TwitterEntity entities = tweet.data.entities;

                    string[] emojis = emojiCache.ContainsEmojis(tweet.data.text).ToArray();

                    //  Need to move this to an options builder or at least a configuration element
                    string[] picHosts = new[] { "pic.twitter.com", "instagram.com" };

                    PicUrlParser p = new();

                    UrlEntity[] imageUrls = entities.urls?.Where(url => picHosts.Contains(p.GetHost(url.display_url)))?.ToArray();
                    UrlEntity[] linkUrls = entities.urls?.Except(imageUrls).ToArray();

                    ITransaction trans = db.CreateTransaction();

                    await Task.WhenAll(
                        QueueEntityDataAsync(trans, "emojis", emojis, s => s),
                        QueueEntityDataAsync(trans, "hashTags", entities.hashtags, e => e.tag),
                        QueueEntityDataAsync(trans, "annotations", entities.annotations, e => e.normalized_text),
                        QueueEntityDataAsync(trans, "mentions", entities.mentions, e => e.username),
                        QueueEntityDataAsync(trans, "images", imageUrls, e => e.expanded_url.Host),
                        QueueEntityDataAsync(trans, "urls", linkUrls, e => e.expanded_url.Host))
                        .ConfigureAwait(false);

                    await trans.ExecuteAsync(CommandFlags.FireAndForget)
                        .ConfigureAwait(false);
                }
            }
            catch (TaskCanceledException)
            {
                logger.CancelRequest();
            }
        }

        /// <summary>
        /// Queues up a data load with the given <see cref="ITransaction"/> to be executed when the <see cref="ITransaction"/> is executed.
        /// </summary>
        /// <typeparam name="TEntity">The type of entity to load data for</typeparam>
        /// <param name="trans">The <see cref="ITransaction"/> on which to queue the data load</param>
        /// <param name="key">The key name to queue data against</param>
        /// <param name="entities">One or More entity to load</param>
        /// <param name="valueFunction">A means of identifying the relevant data on the entity</param>
        /// <returns></returns>
        private static async Task QueueEntityDataAsync<TEntity>(ITransaction trans, string key, TEntity[] entities, Func<TEntity, string> valueFunction)
        {
            if (entities is null || entities.Length == 0) 
                return;

            await trans.StringIncrementAsync($"tweetsWith{key.RaiseFirstChar()}", flags: CommandFlags.FireAndForget)
                .ConfigureAwait(false);

            for (int i = 0, j = entities.Length; i < j; i++)
            {
                await trans.StringIncrementAsync($"{key}Count", flags: CommandFlags.FireAndForget)
                    .ConfigureAwait(false);

                await trans.HashIncrementAsync(key, valueFunction(entities[i]), flags: CommandFlags.FireAndForget)
                    .ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Load up our Emoji master list
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task LoadEmojiDataAsync(CancellationToken cancellationToken)
        {
            try
            {
                logger.EmojisLoading();

                emojiCache = await client.DownloadEmojisAsync(cancellationToken)
                    .ConfigureAwait(false);

                logger.EmojisLoaded(emojiCache.Count);
            }
            catch (TaskCanceledException)
            {
                logger.CancelRequest();
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.Shutdown(nameof(TweetWorker));

            return Task.CompletedTask;
        }
    }
}