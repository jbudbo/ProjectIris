﻿using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Worker.Clients
{
    using Interfaces;
    using Models;

    /// <summary>
    /// An emoji client targeted towards the jsDeliver CDN
    /// </summary>
    internal sealed class JsDeliverClient : IEmojiClient
    {
        private const string PRIMARY_HOST = "https://cdn.jsdelivr.net";
        private const string PRIMARY_RESOURCE = "/npm/emoji-datasource-twitter/emoji.json";

        private readonly ILogger<JsDeliverClient> logger;
        private readonly IOptions<EmojiClientOptions> options;
        private readonly HttpClient baseClient;

        public JsDeliverClient(HttpClient httpClient, IOptions<EmojiClientOptions> options, ILogger<JsDeliverClient> logger)
        {
            this.logger = logger;
            this.options = options;
            baseClient = httpClient;

            //  If we didn't get a client with a base address, use our primary host
            baseClient.BaseAddress ??= new Uri(options.Value.Host ?? PRIMARY_HOST);
        }

        /// <summary>
        /// Downloads emoji data from jsDeliver and provides it back as a <see cref="EmojiMasterList"/>
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<EmojiMasterList> DownloadEmojisAsync(CancellationToken cancellationToken = default)
        {
            string resource = options.Value.Resource ?? PRIMARY_RESOURCE;

            logger.DownloadingEmojiData(baseClient.BaseAddress, resource);

            await using Stream s = await baseClient.GetStreamAsync(resource, cancellationToken)
                .ConfigureAwait(false);

            var emojiData = await JsonSerializer.DeserializeAsync<EmojiData[]>(s, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            return new EmojiMasterList(emojiData);
        }
    }
}