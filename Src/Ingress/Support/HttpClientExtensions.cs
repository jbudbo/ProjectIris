using Ingress.Clients;
using Ingress.Interfaces;

namespace Microsoft.Extensions.DependencyInjection
{
    using Configuration;
    using System;

    internal static class HttpClientExtensions
    {
        internal static IServiceCollection AddTwitterClient(this IServiceCollection services
            , IConfiguration config)
        {
            services.AddHttpClient<ITwitterClient, TwitterClient>()
                .ConfigureHttpClient((_, client) =>
                {
                    client.BaseAddress = new Uri("https://api.twitter.com");
                    client.DefaultRequestHeaders.Authorization = new("Bearer", config["IRIS_TWITTER_BEARER"]);
                });
            return services;
        }
    }
}
