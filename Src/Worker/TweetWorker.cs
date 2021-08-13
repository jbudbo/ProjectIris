using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Worker
{
    internal sealed class TweetWorker : IHostedService
    {
        private readonly ILogger<TweetWorker> logger;
        private readonly IConnectionMultiplexer redis;

        public TweetWorker(
            ILogger<TweetWorker> logger,
            IConnectionMultiplexer redis)
        {
            this.logger = logger;
            this.redis = redis;
        }

        //private void TweetReceived(object sender, BasicDeliverEventArgs e)
        //{
        //    Utf8JsonReader utf8JsonReader = new Utf8JsonReader(e.Body.Span);

        //    Tweet x = JsonSerializer.Deserialize<Tweet>(ref utf8JsonReader);

        //    logger.LogInformation("Tweet {0} received: {1}", x.data.id, x.data.text);

        //    if (!db.StringSet(x.data.id, x.data.text))
        //        logger.LogWarning("Unable to persist tweet");
        //}

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Tweet worker coming up");

            IDatabase db = redis.GetDatabase();

            while (!cancellationToken.IsCancellationRequested)
            {
                var tweet = await db.ListLeftPopAsync("tweets");

                logger.LogInformation("Tweet Received: {0}", tweet);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Tweet worker going down");


            return Task.CompletedTask;
        }
    }
}