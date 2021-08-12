using System;

namespace Ingress.Models
{
    internal sealed class TwitterOptions
    {
        public Uri ApiUrl { get; private set; }

        public void SetApiUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return;

            ApiUrl = new Uri(url);
        }
    }
}
