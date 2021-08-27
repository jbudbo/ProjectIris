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
        private readonly IHttpClientFactory httpClientFactory;

        public TwitterClient(IHttpClientFactory httpClientFactory, ILogger<TwitterClient> logger, IOptions<TwitterOptions> options)
        {
            this.logger = logger;
            this.options = options;
            this.httpClientFactory = httpClientFactory;
        }

        public async Task StartAsync(Uri uri, Func<string, Task> OnTweet, CancellationToken cancellationToken)
        {
            logger.Connecting(uri);

            try
            {
                var queryParams = options?.Value?.ApiParameters ?? new Dictionary<string,string>();

                if (!queryParams.ContainsKey("tweet.fields"))
                    queryParams["tweet.fields"] = "entities";

                if (!queryParams.ContainsKey("media.fields"))
                    queryParams["media.fields"] = "type,url";

                UriBuilder builder = new("https://api.twitter.com")
                {
                    Path = uri?.OriginalString ?? options?.Value?.ApiUrl?.OriginalString,
                    Query = string.Join('&', queryParams.Select(p => $"{p.Key}={p.Value}"))
                };

                using HttpClient baseClient = httpClientFactory.CreateClient();
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

                        default: // Otherwise we really can't do much else here

                            return;
                    }
                }
                await using Stream s = await response.Content.ReadAsStreamAsync(cancellationToken)
                    .ConfigureAwait(false);

                using StreamReader streamReader = new(s, Encoding.UTF8);

                while (!cancellationToken.IsCancellationRequested && !streamReader.EndOfStream)
                {
                    var line = await streamReader.ReadLineAsync()
                        .ConfigureAwait(false);
                    await (OnTweet?.Invoke(line) ?? Task.CompletedTask);
                }
            }
            catch (TaskCanceledException)
            {
                logger.CancelRequest();
            }
        }
    }
}
