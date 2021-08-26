using StackExchange.Redis;
using System.Text.Json;

namespace Dashboard.Middleware;

internal sealed class ServerSentEventsMiddleware
{
    private readonly RequestDelegate next;
    private readonly IConnectionMultiplexer redis;

    public ServerSentEventsMiddleware(RequestDelegate next, IConnectionMultiplexer redis)
    {
        this.next = next;
        this.redis = redis;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context?.Request is null || context.Request.Headers.Accept != "text/event-stream")
        {
            await next(context!);
            return;
        }

        CancellationToken token = context.RequestAborted;
        IDatabase db = redis.GetDatabase();

        var tweetStart = await db.StringGetAsync("tweetStart");

        var ticksSinceStart = tweetStart.TryParse(out long ticks) ? ticks : 0;

        DateTime timeOfweetStart = new(ticksSinceStart, DateTimeKind.Utc);

        try
        {
            var response = context.Response;

            response.Headers.Add("Cache-Control", "no-cache");
            response.Headers.Add("Content-Type", "text/event-stream");

            while (!context.RequestAborted.IsCancellationRequested)
            {
                TimeSpan secondsSinceStart = DateTime.UtcNow - timeOfweetStart;

                ITransaction transaction = db.CreateTransaction();

                var tTweetCount = transaction.StringGetAsync("tweetCount");

                var tTweetsWithUrls = transaction.StringGetAsync("tweetsWithUrls");
                var tUrlCount = transaction.StringGetAsync("urlsCount");

                var tTweetsWithImages = transaction.StringGetAsync("tweetsWithImages");
                var tPicCount = transaction.StringGetAsync("imagesCount");

                var tTweetsWithEmojis = transaction.StringGetAsync("tweetsWithEmojis");
                var tEmojiCount = transaction.StringGetAsync("emojisCount");

                var tTweetsWithHashtags = transaction.StringGetAsync("tweetsWithHashTags");
                var tHashtagCount = transaction.StringGetAsync("hashTagsCount");

                var tTweetsWithMentions = transaction.StringGetAsync("tweetsWithMentions");
                var tMentionCount = transaction.StringGetAsync("mentionsCount");

                var tDomainLeaders = transaction.HashGetAllAsync("urls");
                var tPicLeaders = transaction.HashGetAllAsync("images");
                var tEmojiLeaders = transaction.HashGetAllAsync("emojis");
                var tHashtagLeaders = transaction.HashGetAllAsync("hashTags");
                var tMentionLeaders = transaction.HashGetAllAsync("mentions");

                await transaction.ExecuteAsync();

                var domainLeaders = await tDomainLeaders;
                var picLeaders = await tPicLeaders;
                var emojiLeaders = await tEmojiLeaders;
                var hashtagLeaders = await tHashtagLeaders;
                var mentionLeaders = await tMentionLeaders;

                double tweetCount = double.TryParse(await tTweetCount, out double tc) ? tc : 0;

                double tweetsWithUrls = double.TryParse(await tTweetsWithUrls, out double twu) ? twu : 0;
                double urlCount = double.TryParse(await tUrlCount, out double uc) ? uc : 0;

                double tweetsWithImages = double.TryParse(await tTweetsWithImages, out double twi) ? twi : 0;
                double picCount = double.TryParse(await tPicCount, out double pc) ? pc : 0;

                double tweetsWithEmojis = double.TryParse(await tTweetsWithEmojis, out double twe) ? twe : 0;
                double emojiCount = double.TryParse(await tEmojiCount, out double ec) ? ec : 0;

                double tweetsWithHashtags = double.TryParse(await tTweetsWithHashtags, out double twh) ? twh : 0;
                double hashtagCount = double.TryParse(await tHashtagCount, out double hc) ? hc : 0;

                double tweetsWithMentions = double.TryParse(await tTweetsWithMentions, out double twm) ? twm : 0;
                double mentionsCount = double.TryParse(await tMentionCount, out double mc) ? mc : 0;

                var anon = new
                {
                    tweetsPerSec = tweetCount / secondsSinceStart.TotalSeconds,
                    tweetsReceived = tweetCount,

                    emojiPerc = (tweetsWithEmojis / tweetCount) * 100.0,
                    topEmojis = emojiLeaders
                        .OrderByDescending(v => v.Value)
                        .Take(5)
                        .Select(e => (e.Name.ToString(), e.Value.TryParse(out double c) ? c : 0))
                        .Select(e => $"{e.Item1} ({e.Item2 / emojiCount * 100.0}%)")
                        .ToArray(),

                    urlPerc = (tweetsWithUrls / tweetCount) * 100.0,
                    topDomains = domainLeaders
                        .OrderByDescending(v => v.Value)
                        .Take(5)
                        .Select(e => (e.Name.ToString(), e.Value.TryParse(out double c) ? c : 0))
                        .Select(e => $"{e.Item1} ({e.Item2 / urlCount * 100.0}%)")
                        .ToArray(),

                    picPerc = (tweetsWithImages / tweetCount) * 100.0,
                    topPicDomains = picLeaders
                        .OrderByDescending(v => v.Value)
                        .Take(5)
                        .Select(e => (e.Name.ToString(), e.Value.TryParse(out double c) ? c : 0))
                        .Select(e => $"{e.Item1} ({e.Item2 / picCount * 100.0}%)")
                        .ToArray(),

                    hashTagPerc = (tweetsWithHashtags / tweetCount) * 100.0,
                    topHashTags = hashtagLeaders
                        .OrderByDescending(v => v.Value)
                        .Take(5)
                        .Select(e => (e.Name.ToString(), e.Value.TryParse(out double c) ? c : 0))
                        .Select(e => $"http://twitter.com/hashtag/{e.Item1} ({e.Item2 / hashtagCount * 100.0}%)")
                        .ToArray(),

                    mentionPerc = (tweetsWithMentions / tweetCount) * 100.0,
                    topMentions = mentionLeaders
                        .OrderByDescending(v => v.Value)
                        .Take(5)
                        .Select(e => (e.Name.ToString(), e.Value.TryParse(out double c) ? c : 0))
                        .Select(e => $"{e.Item1} ({e.Item2 / picCount * 100.0}%)")
                        .ToArray()
                };

                await WriteTweetEventAsync(response, token);
                await WriteTweetDataAsync(response, anon, token);
            }
        }
        catch (TaskCanceledException) { }
    }

    /// <summary>
    /// Writes a New Tweet Event to the SSE stream
    /// </summary>
    /// <param name="response"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    private static Task WriteTweetEventAsync(HttpResponse response, CancellationToken token = default)
        => response.WriteAsync("event: New Tweet data\r\n", token);

    /// <summary>
    /// Writes a set of
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    /// <param name="response">The <see cref="HttpResponse"/> to write data to</param>
    /// <param name="data">The data to serialize out to the response</param>
    /// <param name="token"></param>
    /// <returns></returns>
    private static async Task WriteTweetDataAsync<TData>(HttpResponse response, TData data, CancellationToken token = default)
    {
        await response.WriteAsync("data: ", token)
            .ConfigureAwait(false);
            
        await JsonSerializer.SerializeAsync(response.Body, data, cancellationToken: token)
            .ConfigureAwait(false);

        await response.WriteAsync("\n\n", token)
            .ConfigureAwait(false);

        await response.Body.FlushAsync(token)
            .ConfigureAwait(false);
    }
}