using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Ingress
{
    using Interfaces;
    using Models;
    

    internal class IngressWorker : IHostedService
    {
        private readonly ILogger<IngressWorker> logger;
        private readonly ITwitterClient client;
        private readonly IOptions<TwitterOptions> options;
        private readonly IConnectionMultiplexer redis;

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

            IDatabase db = redis.GetDatabase();

            Task onTweet(string tweet)
            {
                logger.TweetReceived(tweet);

                //  Don't even bother serializing that way we can hoover as much data as possible
                return db.ListLeftPushAsync("tweets", tweet);
            }

            await client.StartAsync(options.Value.ApiUrl, onTweet, cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.Shutdown();

            client.Drop();

            return Task.CompletedTask;
        }
    }
}