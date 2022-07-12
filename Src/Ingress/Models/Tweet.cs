using Confluent.Kafka;
using Google.Protobuf;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ingress.Models;

public struct TweetUrl
{
    public uint start { get; set; }
    public uint end { get; set; }
    public string url { get; set; }
    public string expanded_url { get; set; }
    public string display_url { get; set; }
}
public struct TweetHashtag
{
    public uint start { get; set; }
    public uint end { get; set; }
    public string tag { get; set; }
}

public struct TweetAnnotation
{
    public uint start { get; set; }
    public uint end { get; set; }
    public float probability { get; set; }
    public string type { get; set; }
    public string normalized_text { get; set; }
}

public struct TweetMention
{
    public uint start { get; set; }
    public uint end { get; set; }
    public string username { get; set; }
    public string id { get; set; }
}

public struct TweetEntity
{
    public IEnumerable<TweetAnnotation> annotations { get; set; }
    public IEnumerable<TweetMention> mentions { get; set; }
    public IEnumerable<TweetHashtag> hashtags { get; set; }
    public IEnumerable<TweetUrl> urls { get; set; }
}

public struct Tweet
{
    public string id { get; set; }
    public string text { get; set; }
    public TweetEntity entities { get; set; }
}

[JsonSerializable(typeof(Tweet))]
internal sealed partial class TweetJsonContext : JsonSerializerContext { }

internal sealed class TweetJsonSerDe : ISerializer<Tweet>
{
    public byte[] Serialize(Tweet data, SerializationContext context)
        => JsonSerializer.SerializeToUtf8Bytes(data, TweetJsonContext.Default.Tweet);
}

internal struct TweetData
{
    public Tweet data { get; set; }
}

[JsonSerializable(typeof(TweetData))]
internal sealed partial class TweetDataJsonContext : JsonSerializerContext { }

internal sealed class TweetDataJsonSerDe : ISerializer<TweetData>
{
    public byte[] Serialize(TweetData data, SerializationContext context)
        => JsonSerializer.SerializeToUtf8Bytes(data, TweetDataJsonContext.Default.TweetData);
}

internal sealed class TweetProtoSerDe : ISerializer<Iris.Proto.Tweet>
{
    public byte[] Serialize(Iris.Proto.Tweet data, SerializationContext context)
    {
        Span<byte> buffer = stackalloc byte[data.CalculateSize()];
        data.WriteTo(buffer);
        return buffer.ToArray();
    }
}