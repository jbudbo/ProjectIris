using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

namespace Worker
{
    using Support;

    sealed class Program
    {
        /// <summary>
        /// Primary Console Start method
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        static Task Main(string[] args) => CreateHostBuilder(args).Build()
            .RunAsync();

        /// <summary>
        /// Our Generic Host builder
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args)
            //  Poke in our NETCORE_ environment variables for later host setup
            .ConfigureHostConfiguration(cfg => cfg.AddEnvironmentVariables("NETCORE_"))
            //  Poke in our IRIS_ configuration points
            .ConfigureAppConfiguration((hbc, cfg) =>
            {
                if (hbc.HostingEnvironment.IsDevelopment())
                    cfg.AddUserSecrets<Program>();

                cfg.AddEnvironmentVariables("IRIS_");
            })
            .ConfigureServices((_, services) =>
            {
                services.AddHostedService<TweetWorker>()
                    .AddRedis();
            });
    }
}
