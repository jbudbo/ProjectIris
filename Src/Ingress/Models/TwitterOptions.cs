using System;

namespace Ingress.Models;

/// <summary>
/// Options for configuring out we interact with the Twitter API
/// </summary>
internal sealed class TwitterOptions
{
    /// <summary>
    /// The base endpoint to reach Twitter
    /// </summary>
    public Uri Base { get; set; }

    /// <summary>
    /// The endpoint of our API at Twitter
    /// </summary>
    public Uri Endpoint { get; set; }

    /// <summary>
    /// Our Bearer token
    /// </summary>
    public string Bearer { get; set; }
}
