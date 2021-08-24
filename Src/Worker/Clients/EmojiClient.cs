using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Worker.Clients
{
    using Interfaces;
    using Models;

    internal sealed class EmojiClient : IEmojiClient
    {
        private readonly ILogger<EmojiClient> logger;
        private readonly HttpClient baseClient;

        public EmojiClient(HttpClient httpClient, ILogger<EmojiClient> logger)
        {
            this.logger = logger;
            baseClient = httpClient;
        }

        public async Task<EmojiMasterList> DownloadEmojisAsync(CancellationToken cancellationToken)
        {
            await using Stream s = await baseClient.GetStreamAsync("/npm/emoji-datasource-twitter/emoji.json", cancellationToken)
                .ConfigureAwait(false);

            var emojiData = await JsonSerializer.DeserializeAsync<EmojiData[]>(s, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            return new EmojiMasterList(emojiData);
        }
    }
}