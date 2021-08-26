using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;

namespace Worker
{
    using Clients;
    using Support;

    sealed class Program
    {
        /// <summary>
        /// Primary Console Start method
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private static Task Main(string[] args) => CreateHostBuilder(args)
            .Build()
            .RunAsync();

        /// <summary>
        /// Our Generic Host builder
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private static IHostBuilder CreateHostBuilder(string[] args) => Host
            .CreateDefaultBuilder(args)
            //  Poke in our NETCORE_ environment variables for later host setup
            .ConfigureHostConfiguration(cfg => cfg.AddEnvironmentVariables("NETCORE_"))
            //  Poke in our IRIS_ configuration points
            .ConfigureAppConfiguration(SetupConfigurationElements)
            .ConfigureServices(SetupServices);

        /// <summary>
        /// Bootstrap our configuration providers
        /// </summary>
        /// <param name="builderContext"></param>
        /// <param name="configurationBuilder"></param>
        private static void SetupConfigurationElements(HostBuilderContext builderContext, IConfigurationBuilder configurationBuilder)
        {
            if (builderContext.HostingEnvironment.IsDevelopment())
                configurationBuilder.AddUserSecrets<Program>();

            configurationBuilder.AddEnvironmentVariables("IRIS_");
        }

        /// <summary>
        /// Bootstrap all of our services
        /// </summary>
        /// <param name="builderContext"></param>
        /// <param name="services"></param>
        private static void SetupServices(HostBuilderContext builderContext, IServiceCollection services) => services
            .AddHostedService<TweetWorker>()
            .AddEmojiClient<JsDeliverClient>(builderContext.Configuration)
            .AddRedis(builderContext.Configuration);
    }
}
