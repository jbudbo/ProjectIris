using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Ingress
{
    using Interfaces;
    using Models;

    /// <summary>
    /// A hosted service designed to intake Tweet data as fast and efficiently as possible
    /// </summary>
    internal class IngressWorker : IHostedService
    {
        private readonly ILogger<IngressWorker> logger;
        private readonly ITwitterClient client;
        private readonly IOptions<TwitterOptions> options;
        private readonly IConnectionMultiplexer redis;

        private IDatabase db;

        public IngressWorker(
            ILogger<IngressWorker> logger, 
            IOptions<TwitterOptions> options,
            IConnectionMultiplexer redis,
            ITwitterClient client)
        {
            this.logger = logger;
            this.client = client;
            this.options = options;
            this.redis = redis;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            logger.Startup();

            db = redis.GetDatabase();

            //  Mark the time of our first tweet so we can calculate rolling averages 
            await db.StringSetAsync("tweetStart", DateTime.UtcNow.Ticks, flags: CommandFlags.FireAndForget)
                .ConfigureAwait(false);

            await client.StartAsync(options.Value.ApiUrl, OnTweetAsync, cancellationToken)
                .ConfigureAwait(false);
        }

        private async Task OnTweetAsync(string tweet)
        {
            //  Somehow we got here without a db...
            if (db is null)
                return;

            logger.TweetReceived(tweet);

            //  Increment our tweet count so that we can determine input performance as needed
            await db.StringIncrementAsync("tweetCount", flags: CommandFlags.FireAndForget)
                .ConfigureAwait(false);

            //  Don't even bother serializing that way we can hoover as much data as possible
            await db.ListLeftPushAsync("tweets", tweet, flags: CommandFlags.FireAndForget)
                .ConfigureAwait(false);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.Shutdown();

            client.Drop();

            return Task.CompletedTask;
        }
    }
}