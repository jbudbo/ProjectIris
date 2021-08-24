using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Worker.Support
{
    internal static class RedisExtensions
    {
        internal static IServiceCollection AddRedis(this IServiceCollection services)
        {
            var multiplexer = ConnectionMultiplexer.Connect("redis,localhost");

            return services.AddSingleton<IConnectionMultiplexer>(multiplexer);
        }
    }
}
