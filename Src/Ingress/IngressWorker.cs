using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Ingress;

using Interfaces;
using Models;

/// <summary>
/// A hosted service designed to intake Tweet data as fast and efficiently as possible
/// </summary>
internal class IngressWorker : BackgroundService
{
    private readonly ILogger<IngressWorker> logger;
    private readonly ITwitterClient client;
    private readonly IProducer<string, Tweet> producer;
    private readonly string topic;

    public IngressWorker(
        ILogger<IngressWorker> logger, 
        ITwitterClient client,
        IProducer<string, Tweet> producer,
        IOptionsSnapshot<IrisOptions> options)
    {
        this.logger = logger;
        this.client = client;
        this.producer = producer;
        topic = options.Value.Sink;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        static IEnumerable<string> generateTweetsForever(StreamReader reader)
        {
            while (!reader.EndOfStream)
            {
                yield return reader.ReadLine();
            }
        }

        var edbo = new ExecutionDataflowBlockOptions
        {
            CancellationToken = stoppingToken,
            SingleProducerConstrained = true,
            EnsureOrdered = false,
            MaxDegreeOfParallelism = Environment.ProcessorCount
        };
        var x = new TransformManyBlock<StreamReader, string>(generateTweetsForever, edbo);

        var y = new TransformBlock<string, Message<string, Tweet>>(static t =>
        {
            if (string.IsNullOrWhiteSpace(t))
                return null;

            var tweet = JsonSerializer.Deserialize(t, TweetDataJsonContext.Default.TweetData);
            return new Message<string, Tweet>
            {
                Key = tweet.data.id,
                Value = tweet.data
            };
        }, edbo);

        var z = new ActionBlock<Message<string, Tweet>>(T =>
        {
            if (T is null) return;

            producer.Produce(topic, T);
        }, edbo);

        var dlo = new DataflowLinkOptions
        {
            PropagateCompletion = true
        };

        using var linkY = x.LinkTo(y, dlo);
        using var linkZ = y.LinkTo(z, dlo);

        using var reader = await client.GetReaderAsync(stoppingToken)
                .ConfigureAwait(false);

        if (reader is null)
            return;

        x.Post(reader);

        stoppingToken.WaitHandle.WaitOne();
    }
}