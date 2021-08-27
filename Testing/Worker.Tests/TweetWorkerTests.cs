using Microsoft.Extensions.Logging;
using Moq;
using StackExchange.Redis;
using Xunit;

namespace Worker.Tests;

using Interfaces;
using Models;
using System.Text.Json;

public class TweetWorkerTests
{
    [Fact]
    [Trait("path", "happy")]
    public async Task Worker_Doesnt_Continue_On_No_Data()
    {
        var mLogger = new Mock<ILogger<TweetWorker>>();

        var mEmojiClient = new Mock<IEmojiClient>();
        mEmojiClient.Setup(c => c.DownloadEmojisAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmojiMasterList(Array.Empty<EmojiData>()));

        var mDatabase = new Mock<IDatabase>();
        mDatabase.Setup(d => d.ListLeftPopAsync("tweets", It.IsAny<CommandFlags>()))
            .ReturnsAsync(new RedisValue(""));

        var mRedis = new Mock<IConnectionMultiplexer>();
        mRedis.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(mDatabase.Object);

        var target = new TweetWorker(mLogger.Object, mEmojiClient.Object, mRedis.Object);
        var source = new CancellationTokenSource(2);

        await target.StartAsync(source.Token);

        mDatabase.Verify(d => d.CreateTransaction(It.IsAny<object>()), Times.Never());
    }

    [Fact]
    [Trait("path", "happy")]
    public async Task Worker_Starts_Transaction_But_Doesnt_Queue_With_Partial_Data()
    {
        var mLogger = new Mock<ILogger<TweetWorker>>();

        var mEmojiClient = new Mock<IEmojiClient>();
        mEmojiClient.Setup(c => c.DownloadEmojisAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmojiMasterList(Array.Empty<EmojiData>()));

        var mTransaction = new Mock<ITransaction>();

        var mDatabase = new Mock<IDatabase>();
        mDatabase.Setup(d => d.ListLeftPopAsync("tweets", It.IsAny<CommandFlags>()))
            .ReturnsAsync(new RedisValue("{\"data\":{\"id\":\"1\",\"text\":\"MOCK\"}}"));
        mDatabase.Setup(d => d.CreateTransaction(It.IsAny<object>()))
            .Returns(mTransaction.Object)
            .Verifiable();

        var mRedis = new Mock<IConnectionMultiplexer>();
        mRedis.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(mDatabase.Object);

        var target = new TweetWorker(mLogger.Object, mEmojiClient.Object, mRedis.Object);
        var source = new CancellationTokenSource(2);

        await target.StartAsync(source.Token);

        mDatabase.Verify(d => d.CreateTransaction(It.IsAny<object>()), Times.AtLeastOnce());
        mTransaction.Verify(t => t.StringIncrementAsync(It.IsAny<RedisKey>(), It.IsAny<long>(), It.IsAny<CommandFlags>()), Times.Never());
        mTransaction.Verify(t => t.HashIncrementAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<long>(), It.IsAny<CommandFlags>()), Times.Never());
    }

    [Fact]
    [Trait("path", "happy")]
    public async Task Worker_Starts_Transaction_And_Queues_Data_When_Entites_Present()
    {
        var mLogger = new Mock<ILogger<TweetWorker>>();

        var mEmojiClient = new Mock<IEmojiClient>();
        mEmojiClient.Setup(c => c.DownloadEmojisAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmojiMasterList(Array.Empty<EmojiData>()));

        var mTransaction = new Mock<ITransaction>();

        MentionEntity mockMention = new(0, 0, "MOCK");
        TwitterEntity twitterEntity = new(null, new[] { mockMention }, null, null);
        TweetData tweetData = new("1", "MOCK", twitterEntity);
        Tweet mockTweet = new(tweetData);
        byte[] tweetBuffer = JsonSerializer.SerializeToUtf8Bytes(mockTweet);

        var mDatabase = new Mock<IDatabase>();
        mDatabase.Setup(d => d.ListLeftPopAsync("tweets", It.IsAny<CommandFlags>()))
            .ReturnsAsync(new RedisValue(System.Text.Encoding.UTF8.GetString(tweetBuffer)));
        mDatabase.Setup(d => d.CreateTransaction(It.IsAny<object>()))
            .Returns(mTransaction.Object)
            .Verifiable();

        var mRedis = new Mock<IConnectionMultiplexer>();
        mRedis.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(mDatabase.Object);

        var target = new TweetWorker(mLogger.Object, mEmojiClient.Object, mRedis.Object);
        var source = new CancellationTokenSource(10);

        await target.StartAsync(source.Token);

        mDatabase.Verify(d => d.CreateTransaction(It.IsAny<object>()), Times.AtLeastOnce());
        mTransaction.Verify(t => t.StringIncrementAsync(It.IsAny<RedisKey>(), It.IsAny<long>(), It.IsAny<CommandFlags>()), Times.AtLeastOnce());
        mTransaction.Verify(t => t.HashIncrementAsync(It.IsAny<RedisKey>(), It.IsAny<RedisValue>(), It.IsAny<long>(), It.IsAny<CommandFlags>()), Times.AtLeastOnce());
    }
}
