using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ingress.Clients
{
    using Interfaces;
    using Models;
    using System.Threading.Channels;

    internal sealed class TwitterClient : ITwitterClient
    {
        private readonly ILogger<TwitterClient> logger;
        private readonly IOptions<TwitterOptions> options;
        private readonly HttpClient baseClient;
        private readonly Channel<string> channel;

        public ChannelReader<string> Reader { get => channel.Reader; }

        public TwitterClient(HttpClient baseClient, ILogger<TwitterClient> logger, IOptions<TwitterOptions> options)
        {
            this.logger = logger;
            this.options = options;
            this.baseClient = baseClient;
            this.channel = Channel.CreateBounded<string>(new BoundedChannelOptions(10000)
            {
                SingleReader = true,
                SingleWriter = true
            });
        }

        public async Task StartAsync(Uri uri, CancellationToken cancellationToken)
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

                    await channel.Writer.WaitToWriteAsync(cancellationToken).ConfigureAwait(false);
                    await channel.Writer.WriteAsync(line, cancellationToken).ConfigureAwait(false);
                }

                channel.Writer.Complete();
            }
            catch (TaskCanceledException)
            {
                logger.CancelRequest();
            }
            catch (Exception e)
            {
                channel.Writer.Complete(e);
            }
        }
    }
}
