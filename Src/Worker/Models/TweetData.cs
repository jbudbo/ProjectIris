namespace Worker.Models
{
    /// <summary>
    /// A Tweet from Twitter consisting of <see cref="TweetData"/>
    /// </summary>
    /// <param name="data"></param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Names are kept lower to ease with JSON Serialization")]
    internal sealed record Tweet(TweetData data);

    /// <summary>
    /// A entry of data consisting of an ID, Text, and set of <see cref="TwitterEntity"/> Metadata
    /// </summary>
    /// <param name="id">The ID of the Tweet</param>
    /// <param name="text">The raw Text of the Tweet</param>
    /// <param name="entities">Additional Tweet Metadata</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Names are kept lower to ease with JSON Serialization")]
    internal sealed record TweetData(string id, string text, TwitterEntity entities);

    /// <summary>
    /// Tweet Metadata consisting of optional <see cref="UrlEntity"/>, <see cref="MentionEntity"/>, <see cref="AnnotationEntity"/>, and <see cref="HashtagEntity"/> metadata
    /// </summary>
    /// <param name="urls">Any Urls from the Tweet</param>
    /// <param name="mentions">Any Mentions from the Tweet</param>
    /// <param name="annotations">Any Annotations from the Tweet</param>
    /// <param name="hashtags">Any Hash Tags from the Tweet</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Names are kept lower to ease with JSON Serialization")]
    internal sealed record TwitterEntity(UrlEntity[] urls, MentionEntity[] mentions, AnnotationEntity[] annotations, HashtagEntity[] hashtags);

    /// <summary>
    /// Metadata about a Url found in a Tweet
    /// </summary>
    /// <param name="start">Where in the Tweet the Url starts</param>
    /// <param name="end">Where in the Tweet the Url ends</param>
    /// <param name="url">The Twitter-Safe Shortened Url</param>
    /// <param name="expanded_url">The full Url </param>
    /// <param name="display_url">The Url to be displayed to the User</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Names are kept lower to ease with JSON Serialization")]
    internal sealed record UrlEntity(int start, int end, Uri url, Uri expanded_url, Uri display_url);

    /// <summary>
    /// Metadata about a Mention found in a Tweet
    /// </summary>
    /// <param name="start">Where in the Tweet the Mention starts</param>
    /// <param name="end">Where in the Tweet the Mention ends</param>
    /// <param name="username">The Twitter user</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Names are kept lower to ease with JSON Serialization")]
    internal sealed record MentionEntity(int start, int end, string username);

    /// <summary>
    /// Metadata about an Annotation found in a Tweet
    /// </summary>
    /// <param name="start">Where in the Tweet the Annotation starts</param>
    /// <param name="end">Where in the Tweet the Annotation ends</param>
    /// <param name="probability">The level of confidence in this annotation</param>
    /// <param name="type">The Type of Annotation this is; Person, Place, etc.</param>
    /// <param name="normalized_text">The normalized (safe) text of the Annotation</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Names are kept lower to ease with JSON Serialization")]
    internal sealed record AnnotationEntity(int start, int end, decimal probability, string type, string normalized_text);

    /// <summary>
    /// Metadata about a Hash Tag found in a Tweet
    /// </summary>
    /// <param name="start">Where in the Tweet the Hash Tag starts</param>
    /// <param name="end">Where in the Tweet the Hash Tag ends</param>
    /// <param name="tag">The HashTag</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Names are kept lower to ease with JSON Serialization")]
    internal sealed record HashtagEntity(int start, int end, string tag);
}