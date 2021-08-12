using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Text;

namespace Ingress
{
    using Interfaces;
    using Models;
    using RabbitMQ.Client.Exceptions;

    internal class IngressWorker : IHostedService
    {
        private readonly ILogger<IngressWorker> logger;
        private readonly ITwitterClient client;
        private readonly IOptions<TwitterOptions> options;
        private readonly IConnectionFactory connectionFactory;
        private IModel channel;

        public IngressWorker(
            ILogger<IngressWorker> logger, 
            IOptions<TwitterOptions> options,
            IConnectionFactory connectionFactory,
            ITwitterClient client)
        {
            this.logger = logger;
            this.client = client;
            this.options = options;
            this.connectionFactory = connectionFactory;
        }

        private static bool TryConnect(IConnectionFactory factory, out IConnection connection)
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

        private void TweetReceived(object sender, string tweet)
        {
            logger.LogInformation("Tweet: {0}...", tweet);
            channel.BasicPublish(string.Empty, "Twitter", body: Encoding.UTF8.GetBytes(tweet));
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Ingress worker coming up");

            logger.LogInformation("Connecting to RabbitMQ");
            //  Tries each host in the list and connects to the first it can find, therefore since we're orchestrated
            //       Let's make happy path a Host by the name of rabbit and if that doesn't work try localhost
            for (int attempts = 1, max = 7; attempts <= max; attempts++)
            {
                if (TryConnect(connectionFactory, out IConnection connection))
                {
                    using (connection)
                    {
                        channel = connection.CreateModel();

                        channel.QueueDeclare("Twitter", true, false, true);
                        client.OnTweet += TweetReceived;

                        await client.ConnectAsync(options.Value.ApiUrl, cancellationToken);

                        return;
                    }
                }
                logger.LogInformation($"Was unable to reach RabbitMQ, will try again in {1000 * attempts} milliseconds");
                await Task.Delay(1000 * attempts, cancellationToken);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Ingress worker going down");

            channel.Close();
            channel.Dispose();

            client.Drop();
            client.OnTweet -= TweetReceived;

            return Task.CompletedTask;
        }
    }
}