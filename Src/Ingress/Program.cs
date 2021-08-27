using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Ingress.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2, PublicKey=0024000004800000940000000602000000240000525341310004000001000100c547cac37abd99c8db225ef2f6c8a3602f3b3606cc9891605d02baa56104f4cfc0734aa39b93bf7852f7d9266654753cc297e7d2edfe0bac1cdcf9f717241550e0a7b191195b7667bb4f64bcb8e2121380fd1d9d46ad2d92d2d15605093924cceaf74c4861eff62abf69b9291ed0a340e113be11e6a7d3113e92484cf7045cc7")]
namespace Ingress
{
    using Models;

    sealed class Program
    {
        /// <summary>
        /// Primary Console Start method
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        static Task Main(string[] args) => CreateHostBuilder(args)
            .Build()
            .RunAsync();

        /// <summary>
        /// Our Generic Host builder
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        static IHostBuilder CreateHostBuilder(string[] args) => Host
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

            configurationBuilder.AddJsonFile("twitter.json", true)
                .AddEnvironmentVariables("IRIS_");
        }

        /// <summary>
        /// Bootstrap all of our services
        /// </summary>
        /// <param name="builderContext"></param>
        /// <param name="services"></param>
        private static void SetupServices(HostBuilderContext builderContext, IServiceCollection services) => services
            .Configure<TwitterOptions>(o => o.SetApiUrl(builderContext.Configuration["TWITTER_ENDPOINT"]))
            .AddHostedService<IngressWorker>()
            .AddRedis(builderContext.Configuration)
            .AddTwitterClient(builderContext.Configuration);

    }
}
