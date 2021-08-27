namespace Worker.Models
{
    /// <summary>
    /// A DTO of Emoji Data used for serialization
    /// </summary>
    /// <param name="name">The Name of the Emoji</param>
    /// <param name="unified">The Unicode Codpoint for the emoji</param>
    /// <param name="non_qualified">The Emoji Variation selector</param>
    /// <param name="category">The Emoji Category</param>
    /// <param name="subcategory">The Emoji SubCategory</param>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Names are kept lower to ease with JSON Serialization")]
    public sealed record EmojiData(
        string name,
        string unified,
        string non_qualified,
        string category,
        string subcategory);
}
