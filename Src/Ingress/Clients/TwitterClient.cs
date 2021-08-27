using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ingress.Clients
{
    using Interfaces;
    using Models;    

    internal sealed class TwitterClient : ITwitterClient
    {
        private readonly ILogger<TwitterClient> logger;
        private readonly IOptions<TwitterOptions> options;
        private readonly HttpClient baseClient;

        public TwitterClient(HttpClient httpClient, ILogger<TwitterClient> logger, IOptions<TwitterOptions> options)
        {
            this.logger = logger;
            this.options = options;
            baseClient = httpClient;
        }

        public async Task StartAsync(Uri uri, Func<string, Task> OnTweet, CancellationToken cancellationToken)
        {
            logger.Connecting(uri);

            try
            {
                var queryParams = options.Value?.ApiParameters;

                if (!queryParams.ContainsKey("tweet.fields"))
                    queryParams["tweet.fields"] = "entities";

                if (!queryParams.ContainsKey("media.fields"))
                    queryParams["media.fields"] = "type,url";

                UriBuilder builder = new("https://api.twitter.com")
                {
                    Path = uri?.OriginalString ?? options?.Value?.ApiUrl?.OriginalString,
                    Query = string.Join('&', queryParams.Select(p => $"{p.Key}={p.Value}"))
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

                using StreamReader streamReader = new(s, Encoding.UTF8);

                while (!cancellationToken.IsCancellationRequested && !streamReader.EndOfStream)
                {
                    await OnTweet(await streamReader.ReadLineAsync()
                        .ConfigureAwait(false));
                }
            }
            catch (TaskCanceledException)
            {
                logger.CancelRequest();
            }
        }

        public void Drop()
        {
            baseClient.CancelPendingRequests();
            baseClient.Dispose();
        }
    }
}
