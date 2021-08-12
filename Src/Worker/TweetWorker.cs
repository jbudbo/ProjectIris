using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using StackExchange.Redis;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Worker
{
    internal sealed class TweetWorker : IHostedService
    {
        private readonly ILogger<TweetWorker> logger;
        private readonly IConnectionFactory connectionFactory;
        private readonly IConnectionMultiplexer redis;
        private IDatabase db;
        private IConnection connection;
        private IModel channel;
        private EventingBasicConsumer consumer;

        public TweetWorker(
            ILogger<TweetWorker> logger,
            IConnectionFactory connectionFactory,
            IConnectionMultiplexer redis)
        {
            this.logger = logger;
            this.connectionFactory = connectionFactory;
            this.redis = redis;
        }

        private static bool TryConnectToRabbit(IConnectionFactory factory, out IConnection connection)
        {
            try
            {
                connection = factory.CreateConnection(new[] { "rabbit", "localhost" }, Environment.MachineName);
                return true;
            }
            catch (BrokerUnreachableException)
            {
                connection = null;
                return false;
            }
        }

        private static bool TryConnectToRedis(IConnectionMultiplexer redis, out IDatabase db)
        {
            try
            {
                db = redis.GetDatabase();
                return true;
            }
            catch (Exception)
            {
                db = null;
                return false;
            }
        }

        private void TweetReceived(object sender, BasicDeliverEventArgs e)
        {
            Utf8JsonReader utf8JsonReader = new Utf8JsonReader(e.Body.Span);

            Tweet x = JsonSerializer.Deserialize<Tweet>(ref utf8JsonReader);

            logger.LogInformation("Tweet {0} received: {1}", x.data.id, x.data.text);

            if (!db.StringSet(x.data.id, x.data.text))
                logger.LogWarning("Unable to persist tweet");
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Tweet worker coming up");

            for (int attempts = 1, max = 7; attempts <= max; attempts++)
            {
                if (TryConnectToRedis(redis, out db))
                {
                    break;
                }
                logger.LogInformation($"Was unable to reach reddis, will try again in {1000 * attempts} milliseconds");
                await Task.Delay(1000 * attempts, cancellationToken);
            }

            for (int attempts = 1, max = 7; attempts <= max; attempts++)
            {
                if (TryConnectToRabbit(connectionFactory, out connection))
                {
                    channel = connection.CreateModel();
                    channel.QueueDeclare("Twitter", true, false, true);

                    consumer = new EventingBasicConsumer(channel);
                    consumer.Received += TweetReceived;
                    channel.BasicConsume("Twitter", true, consumer);
                    return;
                }
                logger.LogInformation($"Was unable to reach RabbitMQ, will try again in {1000 * attempts} milliseconds");
                await Task.Delay(1000 * attempts, cancellationToken);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Tweet worker going down");

            consumer.Received -= TweetReceived;

            channel.Close();
            channel.Dispose();

            connection.Close();
            connection.Dispose();

            return Task.CompletedTask;
        }
    }
}