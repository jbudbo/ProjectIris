﻿using System;
using System.IO;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Ingress.Clients
{
    using Interfaces;
    using System.Threading;

    internal sealed class TwitterClient : ITwitterClient
    {
        private readonly ILogger<TwitterClient> logger;
        private readonly HttpClient baseClient;

        public TwitterClient(HttpClient httpClient, ILogger<TwitterClient> logger)
        {
            this.logger = logger;
            baseClient = httpClient;
        }

        public event EventHandler<string> OnTweet;

        public async Task ConnectAsync(Uri uri, CancellationToken cancellationToken)
        {
            logger.LogInformation($"Connecting to {uri}");
            await using Stream s = await baseClient.GetStreamAsync(uri, cancellationToken);
            await using BufferedStream bs = new (s);
            using StreamReader streamReader = new (bs, Encoding.UTF8);

            while (!cancellationToken.IsCancellationRequested && !streamReader.EndOfStream)
            {
                OnTweet(this, await streamReader.ReadLineAsync());
            }
        }

        public void Drop()
        {
            baseClient.CancelPendingRequests();
            baseClient.Dispose();
        }
    }
}