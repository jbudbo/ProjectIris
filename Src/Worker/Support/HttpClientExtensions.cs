using Worker.Interfaces;
using Worker.Models;

namespace Microsoft.Extensions.DependencyInjection
{
    using Configuration;

    /// <summary>
    /// Extensions for the <see cref="IServiceCollection"/> interface
    /// </summary>
    internal static class HttpClientExtensions
    {
        /// <summary>
        /// Adds a given <see cref="IEmojiClient"/> to the service collection
        /// </summary>
        /// <typeparam name="TClient">The <see cref="IEmojiClient"/> to add</typeparam>
        /// <param name="services"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        internal static IServiceCollection AddEmojiClient<TClient>(this IServiceCollection services, IConfiguration config)
            where TClient : class, IEmojiClient
        {
            services.AddOptions<EmojiClientOptions>();

            services.AddHttpClient<IEmojiClient, TClient>()
                .ConfigureHttpClient(newClient =>
                {
                    string configAddress = config["EMOJI_HOST"];

                    if (string.IsNullOrWhiteSpace(configAddress)
                        || !Uri.IsWellFormedUriString(configAddress, UriKind.Absolute))
                    {
                        //  Got no or a bad Uri from config, don't use it
                        return;
                    }

                    newClient.BaseAddress = new Uri(configAddress);
                });

            return services;
        }
    }
}
