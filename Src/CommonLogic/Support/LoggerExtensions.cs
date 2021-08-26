using System.Runtime.CompilerServices;

namespace Microsoft.Extensions.Logging
{
    internal static partial class LoggerExtensions
    {
        private static readonly Action<ILogger, string?, Exception?> startup = LoggerMessage
            .Define<string?>(LogLevel.Information, new(1, nameof(Startup)), "{caller} coming up");

        private static readonly Action<ILogger, string?, Exception?> shutdown = LoggerMessage
            .Define<string?>(LogLevel.Information, new(2, nameof(Shutdown)), "{caller} is shutting down");

        /// <summary>
        /// Logs a Startup event
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="caller"></param>
        internal static void Startup(this ILogger logger, [CallerMemberName] string? caller = null) => startup(logger, caller, null);

        /// <summary>
        /// Logs a Shutdown event
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="caller"></param>
        internal static void Shutdown(this ILogger logger, [CallerMemberName] string? caller = null) => shutdown(logger, caller, null);
    }
}
