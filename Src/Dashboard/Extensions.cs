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

            var redis = ctxt.RequestServices.GetRequiredService<IConnectionMultiplexer>();

            IDatabase db = redis.GetDatabase();

            string reqPath = ctxt.Request?.Path.Value ?? string.Empty;

            if (!reqPath.Equals("/datafeed", StringComparison.InvariantCultureIgnoreCase))
            {
                await next();
                return;
            }

            try
            {
                var response = ctxt.Response;

                response.Headers.Add("Cache-Control", "no-cache");
                response.Headers.Add("Content-Type", "text/event-stream");

                while(!ctxt.RequestAborted.IsCancellationRequested)
                {
                    ITransaction transaction = db.CreateTransaction();

                    var tTweetCount = transaction.StringGetAsync("tweetCount");

                    await transaction.ExecuteAsync();

                    var anon = new { tweetsReceived = (await tTweetCount).ToString() };

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
