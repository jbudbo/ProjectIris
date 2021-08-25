using StackExchange.Redis;
using System.Text.Json;

namespace Microsoft.Extensions.DependencyInjection;

internal static class Extensions
{
    public static IServiceCollection AddRedis(this IServiceCollection services)
    {
        var multiplexer = ConnectionMultiplexer.Connect("redis,localhost");
        return services.AddSingleton<IConnectionMultiplexer>(multiplexer);
    }

    public static WebApplication? UseServerSentEvents(this WebApplication? app)
    { 
        app?.Use(async (ctxt, next) =>
        {
            if (ctxt is null)
            {
                await next();
                return;
            }
            
            string reqPath = ctxt.Request?.Path.Value ?? string.Empty;

            if (!reqPath.Equals("/datafeed", StringComparison.InvariantCultureIgnoreCase))
            {
                await next();
                return;
            }

            var redis = ctxt.RequestServices.GetRequiredService<IConnectionMultiplexer>();

            IDatabase db = redis.GetDatabase();

            var tweetStart = await db.StringGetAsync("tweetStart");

            var ticksSinceStart = tweetStart.TryParse(out long ticks) ? ticks : 0;

            DateTime timeSinceTweetStart = new(ticksSinceStart, DateTimeKind.Utc);

            try
            {
                var response = ctxt.Response;

                response.Headers.Add("Cache-Control", "no-cache");
                response.Headers.Add("Content-Type", "text/event-stream");

                while(!ctxt.RequestAborted.IsCancellationRequested)
                {
                    TimeSpan secondsSinceStart = DateTime.UtcNow - timeSinceTweetStart;

                    ITransaction transaction = db.CreateTransaction();

                    var tTweetCount = transaction.StringGetAsync("tweetCount");

                    var tTweetsWithUrls = transaction.StringGetAsync("tweetsWithUrls");
                    var tUrlCount = transaction.StringGetAsync("urlCount");

                    var tTweetsWithImages = transaction.StringGetAsync("tweetsWithImages");
                    var tPicCount = transaction.StringGetAsync("imageCount");

                    var tTweetsWithEmojis = transaction.StringGetAsync("tweetsWithEmojis");
                    var tEmojiCount = transaction.StringGetAsync("emojiCount");

                    var tTweetsWithHashtags= transaction.StringGetAsync("tweetsWithHashtags");
                    var tHashtagCount = transaction.StringGetAsync("hashTagCount");

                    var tDomainLeaders = transaction.HashGetAllAsync("domains");
                    var tPicLeaders = transaction.HashGetAllAsync("picDomains");
                    var tEmojiLeaders = transaction.HashGetAllAsync("emojis");
                    var tHashtagLeaders = transaction.HashGetAllAsync("hashtags");

                    await transaction.ExecuteAsync();

                    var domainLeaders = await tDomainLeaders;
                    var picLeaders = await tPicLeaders;
                    var emojiLeaders = await tEmojiLeaders;
                    var hashtagLeaders = await tHashtagLeaders;

                    double tweetCount = double.TryParse(await tTweetCount, out double tc) ? tc : 0;

                    double tweetsWithUrls = double.TryParse(await tTweetsWithUrls, out double twu) ? twu : 0;
                    double urlCount = double.TryParse(await tUrlCount, out double uc) ? uc : 0;

                    double tweetsWithImages = double.TryParse(await tTweetsWithImages, out double twi) ? twi : 0;
                    double picCount = double.TryParse(await tPicCount, out double pc) ? pc : 0;

                    double tweetsWithEmojis = double.TryParse(await tTweetsWithEmojis, out double twe) ? twe : 0;
                    double emojiCount = double.TryParse(await tEmojiCount, out double ec) ? ec : 0;

                    double tweetsWithHashtags = double.TryParse(await tTweetsWithHashtags, out double twh) ? twh : 0;
                    double hashtagCount = double.TryParse(await tHashtagCount, out double hc) ? hc : 0;

                    var anon = new {
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
                            .Select(e => $"{e.Item1} ({e.Item2 / hashtagCount * 100.0}%)")
                            .ToArray()
                    };

                    await response!.WriteAsync("data: ", ctxt.RequestAborted);

                    await JsonSerializer.SerializeAsync(response.Body, anon);

                    await response!.WriteAsync("\n\n", ctxt.RequestAborted);

                    await response!.Body!.FlushAsync(ctxt.RequestAborted);
                }
            }
            catch (TaskCanceledException) { }
        });

        return app;
    }
}
