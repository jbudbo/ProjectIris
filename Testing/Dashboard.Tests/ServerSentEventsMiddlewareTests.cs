using Microsoft.AspNetCore.Http;
using Moq;
using StackExchange.Redis;
using Xunit;

namespace Dashboard.Tests
{
    using Middleware;
    using System.IO.Pipelines;

    public class ServerSentEventsMiddlewareTests
    {
        [Fact]
        public async Task Middleware_Ignores_Requests_Without_Accept_Header()
        {
            var mRedis = new Mock<IConnectionMultiplexer>();

            var mContext = new Mock<HttpContext>();

            bool pass = false;
            Task next(HttpContext c)
            {
                pass = true;
                return Task.CompletedTask;
            }

            var target = new ServerSentEventsMiddleware(next, mRedis.Object);

            await target.InvokeAsync(mContext.Object);

            Assert.True(pass);
            mRedis.Verify(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>()), Times.Never());
        }

        [Fact]
        public async Task Middleware_Executes_Server_Sent_Event_Requests()
        {
            var mTransaction = new Mock<ITransaction>();

            var mDatabase = new Mock<IDatabase>();
            mDatabase.Setup(d => d.CreateTransaction(It.IsAny<object>())).Returns(mTransaction.Object);
            mDatabase.Setup(d => d.StringGetAsync("tweetStart", It.IsAny<CommandFlags>())).ReturnsAsync(10);
            mDatabase.Setup(d => d.StringGetAsync("tweetCount", It.IsAny<CommandFlags>())).ReturnsAsync(20);

            var mRedis = new Mock<IConnectionMultiplexer>();
            mRedis.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(mDatabase.Object);

            var mHeaders = new Mock<IHeaderDictionary>();
            mHeaders.SetupGet(h => h.Accept).Returns("text/event-stream");

            await using MemoryStream ms = new ();
            PipeWriter pw = PipeWriter.Create(ms);
            var mResponse = new Mock<HttpResponse>();
            mResponse.SetupGet(r => r.Headers).Returns(mHeaders.Object);
            mResponse.SetupGet(r => r.Body).Returns(ms);
            mResponse.SetupGet(r => r.BodyWriter).Returns(pw);

            var mRequest = new Mock<HttpRequest>();
            mRequest.SetupGet(r => r.Headers).Returns(mHeaders.Object);

            CancellationTokenSource source = new(10);

            var mContext = new Mock<HttpContext>();
            mContext.SetupGet(c => c.RequestAborted).Returns(source.Token);
            mContext.SetupGet(c => c.Request).Returns(mRequest.Object);
            mContext.SetupGet(c => c.Response).Returns(mResponse.Object);

            bool pass = false;
            Task next(HttpContext c)
            {
                pass = true;
                return Task.CompletedTask;
            }

            var target = new ServerSentEventsMiddleware(next, mRedis.Object);

            await target.InvokeAsync(mContext.Object);

            Assert.False(pass);
            mRedis.Verify(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>()), Times.Once());
        }
    }
}
