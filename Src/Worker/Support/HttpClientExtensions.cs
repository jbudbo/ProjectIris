using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Worker.Support
{
    using Clients;
    using Interfaces;

    internal static class HttpClientExtensions
    {
        internal static IServiceCollection AddEmojiClient(this IServiceCollection services, IConfiguration config)
        {
            services.AddHttpClient<IEmojiClient, EmojiClient>()
                .ConfigureHttpClient((_, client) =>
                {
                    client.BaseAddress = new Uri("https://cdn.jsdelivr.net");
                });
            return services;
        }
    }
}
