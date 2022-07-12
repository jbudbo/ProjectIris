using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("Ingress.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2, PublicKey=0024000004800000940000000602000000240000525341310004000001000100c547cac37abd99c8db225ef2f6c8a3602f3b3606cc9891605d02baa56104f4cfc0734aa39b93bf7852f7d9266654753cc297e7d2edfe0bac1cdcf9f717241550e0a7b191195b7667bb4f64bcb8e2121380fd1d9d46ad2d92d2d15605093924cceaf74c4861eff62abf69b9291ed0a340e113be11e6a7d3113e92484cf7045cc7")]
namespace Ingress;

using Microsoft.Extensions.Logging;
using Models;

sealed class Program
{
    Program()
    {}

    /// <summary>
    /// Primary Console Start method
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    static async Task Main(string[] args) => await CreateHostBuilder(args)
        .Build()
        .RunAsync()
        .ConfigureAwait(false);

    /// <summary>
    /// Our Generic Host builder
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    static IHostBuilder CreateHostBuilder(string[] args) => Host
        .CreateDefaultBuilder(args)
        //  Poke in our NETCORE_ environment variables for later host setup
        .ConfigureHostConfiguration(cfg => cfg.AddEnvironmentVariables("NETCORE"))
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
#if DEBUG
        configurationBuilder.AddUserSecrets<Program>();
#endif

        configurationBuilder
            .AddJsonFile("/run/secrets/twitter_bearer", true)
            .AddEnvironmentVariables("TWITTER");
    }

    /// <summary>
    /// Bootstrap all of our services
    /// </summary>
    /// <param name="builderContext"></param>
    /// <param name="services"></param>
    private static void SetupServices(HostBuilderContext builderContext, IServiceCollection services)
    {
        services
            .AddOptions<TwitterOptions>()
            .BindConfiguration("TWITTER")
            .Configure<ILogger<TwitterOptions>>(static (o, l) => l.LogDebug("Base: {Base}\nEndpoint: {Endpoint}\nBearer: {Bearer}", o.Base, o.Endpoint, o.Bearer))
            .ValidateOnStart();

        services
            .AddOptions<ProducerConfig>()
            .BindConfiguration("KAFKA")
            .Configure<ILogger<ProducerConfig>>(static (o, l) => l.LogDebug("Broker: {Broker}\nClient: {Client}", o.BootstrapServers, o.ClientId))
            .ValidateOnStart();

        services
            .AddOptions<IrisOptions>()
            .BindConfiguration("IRIS")
            .Configure<ILogger<IrisOptions>>(static (o, l) => l.LogDebug("Sink: {Sink}", o.Sink))
            .ValidateOnStart();

        services
            .AddHostedService<IngressWorker>()
            .AddTwitterClient()
            .AddSingleton(static sp =>
            {
                ProducerConfig config = sp.GetRequiredService<IOptions<ProducerConfig>>().Value;
                return new ProducerBuilder<string, Tweet>(config)
                    .SetValueSerializer(new TweetJsonSerDe())
                    .Build();
            });
    }
}
