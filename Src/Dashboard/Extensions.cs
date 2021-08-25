﻿using StackExchange.Redis;
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
                    var tUrlCount = transaction.StringGetAsync("urlCount");
                    var tEmojiCount = transaction.StringGetAsync("emojiCount");
                    var tdomainLeaders = transaction.SortAsync("domains", take: 5, order: Order.Descending);

                    await transaction.ExecuteAsync();

                    var domainLeaders = await tdomainLeaders;

                    double tweetCount = double.Parse(await tTweetCount);
                    double urlCount = double.Parse(await tUrlCount);
                    double emojiCount = double.Parse(await tEmojiCount);

                    var anon = new {
                        tweetsPerSec = tweetCount / secondsSinceStart.TotalSeconds,
                        tweetsReceived = tweetCount,
                        emojiPerc = (emojiCount / tweetCount) * 100.0,
                        urlPerc = (urlCount / tweetCount) * 100.0,
                        topDomains = string.Join(',', domainLeaders.Select(v => v.ToString()))
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
