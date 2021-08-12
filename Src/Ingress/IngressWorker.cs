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

    internal class IngressWorker : IHostedService
    {
        private readonly ILogger<IngressWorker> logger;
        private readonly ITwitterClient client;
        private readonly IOptions<TwitterOptions> options;
        private readonly IConnection connection;
        private readonly IModel channel;

        public IngressWorker(
            ILogger<IngressWorker> logger, 
            IOptions<TwitterOptions> options,
            IConnectionFactory connectionFactory,
            ITwitterClient client)
        {
            this.logger = logger;
            this.client = client;
            this.options = options;

            logger.LogInformation("Connecting to RabbitMQ");
            //  Tries each host in the list and connects to the first it can find, therefore since we're orchestrated
            //       Let's make happy path a Host by the name of rabbit and if that doesn't work try localhost
            connection = connectionFactory.CreateConnection(new[] { "rabbit", "localhost" }, Environment.MachineName);
            channel = connection.CreateModel();
            channel.QueueDeclare("Twitter", true, false, true);
        }
        private void TweetReceived(object sender, string tweet)
        {
            logger.LogInformation("Tweet: {0}...", tweet);
            channel.BasicPublish(string.Empty, "Twitter", body: Encoding.UTF8.GetBytes(tweet));
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Ingress worker coming up");

            client.OnTweet += TweetReceived;

            return client.ConnectAsync(options.Value.ApiUrl, cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Ingress worker going down");

            client.Drop();
            client.OnTweet -= TweetReceived;

            channel.Close();
            channel.Dispose();

            connection.Close();
            connection.Dispose();

            return Task.CompletedTask;
        }
    }
}