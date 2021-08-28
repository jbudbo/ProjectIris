using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;

namespace Ingress.Tests;

using Clients;
using Models;
using System.Net;

public class TwitterClientTests
{
    [Fact]
    [Trait("path", "happy")]
    public async Task Client_Stands_Up_To_Minimal_Input()
    {
        var mLogger = Mock.Of<ILogger<TwitterClient>>();
        var mOptions = Mock.Of<IOptions<TwitterOptions>>();

        var mFactory = new Mock<IHttpClientFactory>();

        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("MOCK")
            });

        HttpClient client = new (handler.Object);

        var target = new TwitterClient(client, mLogger, mOptions);
        var source = new CancellationTokenSource();

        bool pass = false;
        Task assertTweet(string tweet)
        {
            Assert.Equal("MOCK", tweet);
            pass = true;
            return Task.CompletedTask;
        }

        await target.StartAsync(null, assertTweet, source.Token);

        Assert.True(pass);
    }

    [Fact]
    [Trait("path", "happy")]
    public async Task Client_Stops_Without_Success()
    {
        var mLogger = Mock.Of<ILogger<TwitterClient>>();
        var mOptions = Mock.Of<IOptions<TwitterOptions>>();

        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound,
                Content = new StringContent("MOCK")
            });

        HttpClient client = new (handler.Object);

        var target = new TwitterClient(client, mLogger, mOptions);
        var source = new CancellationTokenSource();

        bool pass = false;
        Task assertTweet(string tweet)
        {
            pass = true;
            return Task.CompletedTask;
        }

        await target.StartAsync(null, assertTweet, source.Token);

        Assert.False(pass);
    }
}