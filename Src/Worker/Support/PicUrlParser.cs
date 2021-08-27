namespace Worker.Support
{
    /// <summary>
    /// Acts as a means of transforming any Relative Picture Uris into Absolute ones
    /// </summary>
    internal class PicUrlParser : HttpStyleUriParser
    {
        /// <summary>
        /// Attemtps to retrieve just the Uri Host of a given Uri even if it's Relative
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public string GetHost(Uri uri)
        {
            if (!uri.IsWellFormedOriginalString())
                return null;

            if (uri.IsAbsoluteUri)
                return uri.Host;

            if (Uri.TryCreate($"http://{uri.OriginalString}", UriKind.Absolute, out Uri u))
                return u.Host;

            return string.Empty;
        }
    }
}
