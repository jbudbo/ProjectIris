using StackExchange.Redis;

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

            if (reqPath.Equals("/datafeed", StringComparison.InvariantCultureIgnoreCase))
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
                    IBatch dataBatch = db.CreateBatch();

                }
            }
            catch (TaskCanceledException) { }
        });

        return app;
    }
}
