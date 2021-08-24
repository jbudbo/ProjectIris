using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System.Net;

namespace Ingress.Support
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
