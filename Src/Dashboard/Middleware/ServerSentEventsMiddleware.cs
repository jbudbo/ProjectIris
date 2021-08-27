using StackExchange.Redis;
using System.Text.Json;

namespace Dashboard.Middleware;

internal sealed class ServerSentEventsMiddleware
{
    private readonly RequestDelegate next;
    private readonly IConnectionMultiplexer redis;

    public ServerSentEventsMiddleware(RequestDelegate next, IConnectionMultiplexer redis)
    {
        this.next = next;
        this.redis = redis;
    }

    /// <summary>
    /// Begin streaming SSE events back to a caller
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task InvokeAsync(HttpContext context)
    {
        if (context?.Request is null || context.Request.Headers.Accept != "text/event-stream")
        {
            await next(context!);
            return;
        }

        CancellationToken token = context.RequestAborted;
        IDatabase db = redis.GetDatabase();

        var tweetStart = await db.StringGetAsync("tweetStart");

        var ticksSinceStart = tweetStart.TryParse(out long ticks) ? ticks : 0;

        DateTime timeOfweetStart = new(ticksSinceStart, DateTimeKind.Utc);

        try
        {
            var response = context.Response;

            response.Headers.Add("Cache-Control", "no-cache");
            response.Headers.Add("Content-Type", "text/event-stream");

            ulong msgId = 0;
            while (!token.IsCancellationRequested)
            {
                TimeSpan secondsSinceStart = DateTime.UtcNow - timeOfweetStart;

                ITransaction transaction = db.CreateTransaction();

                var tweetCountTask = transaction.StringGetAsync("tweetCount");

                var urlsTask = GetTweetDataAsync(transaction, "urls");
                var imagesTask = GetTweetDataAsync(transaction, "images");
                var emojisTask = GetTweetDataAsync(transaction, "emojis");
                var hashTagsTask = GetTweetDataAsync(transaction, "hashTags", Format: "<a href=\"https://twitter.com/hashtag/{0}\" target=\"_blank\">{0}</a> ({1}%)");
                var mentionsTask = GetTweetDataAsync(transaction, "mentions", Format: "<a href=\"https://twitter.com/{0}\" target=\"_blank\">{0}</a> ({1}%)");

                await Task.WhenAll(tweetCountTask, urlsTask, imagesTask, emojisTask, hashTagsTask, mentionsTask, transaction.ExecuteAsync())
                    .ConfigureAwait(false);

                double tweetCount = double.TryParse(tweetCountTask.GetAwaiter().GetResult(), out double tc) ? tc : 0;

                IDictionary<string, object> result = new Dictionary<string, object>
                {
                    [nameof(tweetCount)] = tweetCount,
                    ["tps"] = tweetCount / secondsSinceStart.TotalSeconds,
                    ["urls"] = urlsTask.GetAwaiter().GetResult(),
                    ["images"] = imagesTask.GetAwaiter().GetResult(),
                    ["emojis"] = emojisTask.GetAwaiter().GetResult(),
                    ["hashTags"] = hashTagsTask.GetAwaiter().GetResult(),
                    ["mentions"] = mentionsTask.GetAwaiter().GetResult(),
                };

                msgId = await WriteAndIncrementIdAsync(response, msgId, token);
                await WriteTweetEventAsync(response, token);
                await WriteTweetDataAsync(response, result, token);
            }
        }
        catch (TaskCanceledException) { }
    }

    /// <summary>
    /// Aggregates all the Tweet data for a given category into a dictionary of common elements
    /// </summary>
    /// <param name="transaction">The <see cref="ITransaction"/> to queue work on</param>
    /// <param name="category">The category of tweet data to aggregate</param>
    /// <param name="depth">The top N numbers of entities to aggregate</param>
    /// <param name="Format">A string format to use when building text representation of entites</param>
    /// <returns></returns>
    private static async Task<IDictionary<string, object>> GetTweetDataAsync(ITransaction transaction, string category
        , int depth = 10, string Format = "{0} ({1}%)")
    {
        Task<RedisValue> overallCountTask = transaction.StringGetAsync($"{category}Count");
        Task<RedisValue> tweetsWithCountTask = transaction.StringGetAsync($"tweetsWith{category.RaiseFirstChar()}");
        Task<HashEntry[]> entitiesTask = transaction.HashGetAllAsync(category);

        await Task.WhenAll(overallCountTask, tweetsWithCountTask, entitiesTask)
            .ConfigureAwait(false);

        double overallCount = double.TryParse(overallCountTask.GetAwaiter().GetResult(), out double oc) ? oc : 0;

        double tweetsWithCount = double.TryParse(tweetsWithCountTask.GetAwaiter().GetResult(), out oc) ? oc : 0;

        string[] entityTexts = entitiesTask.GetAwaiter().GetResult()
            .OrderByDescending(v => v.Value)
            .Take(depth)
            .Select(e => (e.Name.ToString(), (e.Value.TryParse(out double c) ? c : 0) / overallCount * 100.0))
            .Select(t => t.ToString(Format))
            .ToArray();

        return new Dictionary<string, object>(3)
        {
            [nameof(overallCount)] = overallCount,
            [nameof(tweetsWithCount)] = tweetsWithCount,
            [nameof(entityTexts)] = entityTexts
        };
    }

    /// <summary>
    /// Writes a New Tweet Event to the SSE stream
    /// </summary>
    /// <param name="response"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    private static Task WriteTweetEventAsync(HttpResponse response, CancellationToken token = default)
        => response.WriteAsync("event: New Tweet data\n\n", token);

    /// <summary>
    /// Writes a new event ID to the SSE stream
    /// </summary>
    /// <param name="response"></param>
    /// <param name="id"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    private static async Task<ulong> WriteAndIncrementIdAsync(HttpResponse response, ulong id, CancellationToken token = default)
    {
        await response.WriteAsync($"id: {id++}\n\n", token);
        return id;
    }

    /// <summary>
    /// Writes a set of
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    /// <param name="response">The <see cref="HttpResponse"/> to write data to</param>
    /// <param name="data">The data to serialize out to the response</param>
    /// <param name="token"></param>
    /// <returns></returns>
    private static async Task WriteTweetDataAsync<TData>(HttpResponse response, TData data, CancellationToken token = default)
    {
        try
        {
            await response.WriteAsync("data: ", token)
                .ConfigureAwait(false);

            await JsonSerializer.SerializeAsync(response.Body, data, cancellationToken: token)
                .ConfigureAwait(false);

            await response.WriteAsync("\n\n", token)
                .ConfigureAwait(false);

            await response.Body.FlushAsync(token)
                .ConfigureAwait(false);
        }
        catch (TaskCanceledException) { }
    }
}