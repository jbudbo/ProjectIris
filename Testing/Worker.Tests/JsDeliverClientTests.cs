using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

namespace Worker.Tests;

using Clients;
using Models;

public class JsDeliverClientTests
{
    [Fact]
    public async Task Client_Stands_Up_To_Minimal_Input()
    {
        var mLogger = Mock.Of<ILogger<JsDeliverClient>>();
        var mOptions = Mock.Of<IOptions<EmojiClientOptions>>();

        var mFactory = new Mock<IHttpClientFactory>();

        var emojiData = new EmojiData("MOCK", "261D-FE0F", string.Empty, string.Empty, string.Empty);
        var emojiBuffer = JsonSerializer.SerializeToUtf8Bytes(new[] { emojiData });

        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(Encoding.UTF8.GetString(emojiBuffer))
            });

        HttpClient client = new (handler.Object);

        var target = new JsDeliverClient(client, mOptions, mLogger);
        var source = new CancellationTokenSource();

        EmojiMasterList list = await target.DownloadEmojisAsync(source.Token);

        Assert.NotEmpty(list);

        string only = list.First();

        Assert.Equal("☝️", only);

        var emojisFromList = list.ContainsEmojis("Don't look ☝️")
            .ToArray();

        Assert.Equal("☝️ - MOCK", emojisFromList[0]);
    }

    [Fact]
    public async Task Client_Handles_No_Emoji_Response()
    {
        var mLogger = Mock.Of<ILogger<JsDeliverClient>>();
        var mOptions = Mock.Of<IOptions<EmojiClientOptions>>();

        var mFactory = new Mock<IHttpClientFactory>();

        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(string.Empty)
            });

        HttpClient client = new (handler.Object);

        var target = new JsDeliverClient(client, mOptions, mLogger);
        var source = new CancellationTokenSource();

        EmojiMasterList list = await target.DownloadEmojisAsync(source.Token);

        Assert.Empty(list);
    }
}