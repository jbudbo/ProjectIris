namespace Worker.Models
{
    internal sealed record Tweet(TweetData data);
    internal sealed record TweetData(string id, string text, TwitterEntity entities);
    internal sealed record TwitterEntity(UrlEntity[] urls, MentionEntity[] mentions, AnnotationEntity[] Annotations, HashtagEntity[] hashtags);
    internal sealed record UrlEntity(int start, int end, Uri url, Uri expanded_url, Uri display_url);
    internal sealed record MentionEntity(int start, int end, string username);
    internal sealed record AnnotationEntity(int start, int end, decimal probability, string type, string normalized_text);
    internal sealed record HashtagEntity(int start, int end, string tag);
}