using Ingress.Clients;
using Ingress.Interfaces;
using Ingress.Models;

namespace Microsoft.Extensions.DependencyInjection;

using Options;

internal static class HttpClientExtensions
{
    internal static IServiceCollection AddTwitterClient(this IServiceCollection services)
    {
        services
            .AddHttpClient<ITwitterClient, TwitterClient>("Twitter")
            .ConfigureHttpClient((sp, client) =>
            {
                var opts = sp.GetRequiredService<IOptionsSnapshot<TwitterOptions>>().Value;
                
                client.BaseAddress = opts.Base;
                client.DefaultRequestHeaders.Authorization = new("Bearer", opts.Bearer);
            });
        return services;
    }
}
