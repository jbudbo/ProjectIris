namespace Worker.Models
{
    /// <summary>
    /// A set of configurable options for any <see cref="Interfaces.IEmojiClient"/>
    /// </summary>
    internal sealed class EmojiClientOptions
    {
        /// <summary>
        /// The URI Hosting Emoji Data
        /// </summary>
        public string Host {  get; set; }

        /// <summary>
        /// Any additional resource path required to access Emoji Data
        /// </summary>
        public string Resource {  get; set; }
    }
}
