using Dashboard.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Microsoft.Extensions.DependencyInjection;

internal static class Extensions
{
    /// <summary>
    /// Maps a given route to handling Server Sent Event requests
    /// </summary>
    /// <param name="app"></param>
    /// <param name="path">The application route</param>
    /// <returns></returns>
    public static IApplicationBuilder MapServerSentEvents(this IApplicationBuilder app, PathString path)
        => app?.Map(path, sseApp => sseApp.UseServerSentEvents())!;

    /// <summary>
    /// Bootstraps SSE middleware into the request pipeline
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static IApplicationBuilder UseServerSentEvents(this IApplicationBuilder app)
        => app?.UseMiddleware<ServerSentEventsMiddleware>()!;
}
