using System.Text;
using Microsoft.Extensions.Logging;

namespace Ingress.Clients
{
    using Interfaces;

    internal sealed class TwitterClient : ITwitterClient
    {
        private readonly ILogger<TwitterClient> logger;
        private readonly HttpClient baseClient;

        public TwitterClient(HttpClient httpClient, ILogger<TwitterClient> logger)
        {
            this.logger = logger;
            baseClient = httpClient;
        }

        public async Task StartAsync(Uri uri, Func<string, Task> OnTweet, CancellationToken cancellationToken)
        {
            logger.Connecting(uri);

            UriBuilder builder = new("https://api.twitter.com")
            {
                Path = uri.OriginalString,
                //  I'd really like to refactor this into a builder so that I can add/remove these choices via other means
                Query = "tweet.fields=entities&media.fields=type,url"
            };

            await using Stream s = await baseClient.GetStreamAsync(builder.Uri, cancellationToken);
            await using BufferedStream bs = new (s);
            using StreamReader streamReader = new (bs, Encoding.UTF8);

            while (!cancellationToken.IsCancellationRequested && !streamReader.EndOfStream)
            {
                await OnTweet(await streamReader.ReadLineAsync());
            }
        }

        public void Drop()
        {
            baseClient.CancelPendingRequests();
            baseClient.Dispose();
        }
    }
}
