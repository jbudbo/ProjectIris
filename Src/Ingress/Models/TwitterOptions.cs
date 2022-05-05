using System;
using System.Collections.Generic;

namespace Ingress.Models
{
    /// <summary>
    /// Options for configuring out we interact with the Twitter API
    /// </summary>
    internal sealed class TwitterOptions
    {
        /// <summary>
        /// The Uri to reach the Twitter API
        /// </summary>
        public Uri ApiUrl { get; private set; }

        /// <summary>
        /// Any additional Parameters to send to the TWitter API
        /// </summary>
        public IDictionary<string, string> ApiParameters { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Sets the API Url from a string
        /// </summary>
        /// <param name="url"></param>
        public void SetApiUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url)
                || !Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute))
                return;

            ApiUrl = new Uri(url, UriKind.Relative);
        }
    }
}
