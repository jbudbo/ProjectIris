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

            using HttpResponseMessage response = await baseClient.GetAsync(builder.Uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false);

            string rateLimit = response.Headers.TryGetValues("x-rate-limit-limit", out var limit) ? limit.FirstOrDefault() : null;
            string rateLimitRemaining = response.Headers.TryGetValues("x-rate-limit-remaining", out var remaining) ? remaining.FirstOrDefault() : null;

            if (!response.IsSuccessStatusCode)
            {
                switch (response.StatusCode)
                {
                    case System.Net.HttpStatusCode.TooManyRequests:
                        //  This would be our opportunity to try again 
                        break;
                }
            }
            await using Stream s = await response.Content.ReadAsStreamAsync(cancellationToken)
                .ConfigureAwait(false);
            //await using BufferedStream bs = new(s);
            using StreamReader streamReader = new(s, Encoding.UTF8);

            while (!cancellationToken.IsCancellationRequested && !streamReader.EndOfStream)
            {
                await OnTweet(await streamReader.ReadLineAsync().ConfigureAwait(false));
            }
        }

        public void Drop()
        {
            baseClient.CancelPendingRequests();
            baseClient.Dispose();
        }
    }
}
