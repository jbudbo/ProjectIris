using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace StackExchange.Redis
{
    using Microsoft.Extensions.Configuration;
    using Worker.Models;

    /// <summary>
    /// Any extension methods for the <see cref="Redis"/> namespace
    /// </summary>
    internal static class RedisExtensions
    {
        /// <summary>
        /// Adds a Redis connection <see cref="IConnectionMultiplexer"/> to the service collection 
        /// specifically looking for a host named redis or localhost
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        internal static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration config)
        {
            services.AddOptions<RedisOptions>()
                .Configure(o => o.Host = config["REDIS_HOST"]);

            return services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var opts = sp.GetRequiredService<IOptions<RedisOptions>>();
                return ConnectionMultiplexer.Connect(string.Join(',', opts.Value.Host?.Trim(), "localhost"));
            });
        }
    }
}
