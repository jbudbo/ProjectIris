using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ingress.Support
{
    using Clients;
    using Interfaces;

    internal static class HttpClientExtensions
    {
        internal static IServiceCollection AddTwitterClient(this IServiceCollection services
            , IConfiguration config)
        {
            services.AddHttpClient<ITwitterClient, TwitterClient>()
                .ConfigureHttpClient((sp, client) =>
                {
                    client.BaseAddress = new Uri("https://api.twitter.com");
                    client.DefaultRequestHeaders.Authorization = new("Bearer", config["IRIS_TWITTER_BEARER"]);
                });
            return services;
        }
    }
}
