using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using StackExchange.Redis;
using Xunit;

namespace Ingress.Tests;

using Interfaces;
using Models;

public class IngressWorkerTests
{
    [Fact]
    [Trait("path","happy")]
    public async Task Worker_Start_Captures_Start_Time_And_Starts_Client()
    {
        var mLogger = Mock.Of<ILogger<IngressWorker>>();
        var mOptions = Mock.Of<IOptions<TwitterOptions>>();

        var mTwitterClient = new Mock<ITwitterClient>();
        mTwitterClient.Setup(c => c.StartAsync(It.IsAny<Uri>(), It.IsAny<Func<string, Task>>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var mData = new Mock<IDatabase>();
        mData.Setup(d => d.StringSetAsync("tweetStart", It.IsAny<RedisValue>(), It.IsAny<TimeSpan>(), It.IsAny<When>(), It.IsAny<CommandFlags>())).ReturnsAsync(true);

        var mRedis = new Mock<IConnectionMultiplexer>();
        mRedis.Setup(e => e.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(mData.Object);

        var target = new IngressWorker(mLogger, mOptions, mRedis.Object, mTwitterClient.Object);
        var tokenSource = new CancellationTokenSource();

        await target.StartAsync(tokenSource.Token);

        mData.Verify(d => d.StringSetAsync("tweetStart", It.IsAny<RedisValue>(), null, It.IsAny<When>(), CommandFlags.FireAndForget), Times.Once());
        mTwitterClient.Verify(c => c.StartAsync(It.IsAny<Uri>(), It.IsAny<Func<string, Task>>(), It.IsAny<CancellationToken>()), Times.Once());
    }
    
    [Fact]
    [Trait("path", "happy")]
    public async Task Worker_Captures_Tweet_Count_As_Well_As_Data_On_Tweet()
    {
        var mLogger = Mock.Of<ILogger<IngressWorker>>();
        var mOptions = Mock.Of<IOptions<TwitterOptions>>();

        var mTwitterClient = new MockTwitterClient();

        var mData = new Mock<IDatabase>();

        var mRedis = new Mock<IConnectionMultiplexer>();
        mRedis.Setup(e => e.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(mData.Object);

        var target = new IngressWorker(mLogger, mOptions, mRedis.Object, mTwitterClient);
        var tokenSource = new CancellationTokenSource();

        await target.StartAsync(tokenSource.Token);

        mData.Verify(d => d.StringIncrementAsync("tweetCount", It.IsAny<long>(), CommandFlags.FireAndForget), Times.Once());
        mData.Verify(d => d.ListLeftPushAsync("tweets", It.IsAny<RedisValue>(), It.IsAny<When>(), CommandFlags.FireAndForget), Times.Once());
    }

    private sealed class MockTwitterClient : ITwitterClient
    {
        public void Drop()
        {
            //  No need to mock this simply because we can verify it directly
        }

        public Task StartAsync(Uri uri, Func<string, Task> OnTweet, CancellationToken cancellationToken = default)
        {
            //  Only truely testing our tweet function here so the rest doesn't truely matter
            return OnTweet(string.Empty);
        }
    }
}