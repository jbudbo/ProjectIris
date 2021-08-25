namespace Worker.Support
{
    internal class PicUrlParser : HttpStyleUriParser
    {
        public string GetHost(Uri uri)
        {
            if (!uri.IsAbsoluteUri)
                uri = Uri.TryCreate($"http://{uri.OriginalString}", UriKind.Absolute, out Uri? u) ? u : uri;

            return GetComponents(uri, UriComponents.Host, UriFormat.Unescaped);
        }
    }
}
