﻿using StackExchange.Redis;
using System.Text.Json;
using Dashboard.Middleware;

namespace Microsoft.Extensions.DependencyInjection;

internal static class Extensions
{
    /// <summary>
    /// Maps a given route to handling Server Sent Event requests
    /// </summary>
    /// <param name="app"></param>
    /// <param name="path">The application route</param>
    /// <returns></returns>
    public static IApplicationBuilder MapServerSentEvents(this IApplicationBuilder app, PathString path)
        => app?.Map(path, sseApp => sseApp.UseServerSentEvents())!;

    /// <summary>
    /// Bootstraps SSE middleware into the request pipeline
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseServerSentEvents(this IApplicationBuilder app)
        => app?.UseMiddleware<ServerSentEventsMiddleware>()!;

    /// <summary>
    /// Add a Server Sent Event handler to the pipeline at a given route
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static WebApplication? UseServerSentEvents2(this WebApplication? app, string route)
    {
        //  If we got no route or a bad route, we're not going to rig anything
        if (string.IsNullOrWhiteSpace(route))
            return app;

        app?.Use((c, n) =>
        {
            if (c?.Request?.Path.Value is null) return n();

            if (c.Request.Path.Value.Equals(route, StringComparison.InvariantCultureIgnoreCase))
                return n();

            return RunSseMiddlewareAsync(c);
        });
        
        return app;
    }

    public static async Task RunSseMiddlewareAsync(HttpContext context)
    {
        var redis = context.RequestServices.GetRequiredService<IConnectionMultiplexer>();

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

                await response!.WriteAsync("data: ", context.RequestAborted);

                await JsonSerializer.SerializeAsync(response.Body, anon);

                await response!.WriteAsync("\n\n", context.RequestAborted);

                await response!.Body!.FlushAsync(context.RequestAborted);
            }
        }
        catch (TaskCanceledException) { }
    }
}
