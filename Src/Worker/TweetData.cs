namespace Worker
{
    internal sealed record Tweet(TweetData data);
    internal sealed record TweetData(string id, string text);
}