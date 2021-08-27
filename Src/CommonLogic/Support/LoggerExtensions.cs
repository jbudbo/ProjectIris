using System.Runtime.CompilerServices;

namespace Microsoft.Extensions.Logging
{
    /// <summary>
    /// Extensions for <see cref="ILogger"/>
    /// </summary>
    internal static partial class LoggerExtensions
    {
        private static readonly Action<ILogger, string, Exception> startup = LoggerMessage
            .Define<string>(LogLevel.Information, new(1, nameof(Startup)), "{caller} coming up");

        private static readonly Action<ILogger, string, Exception> shutdown = LoggerMessage
            .Define<string>(LogLevel.Information, new(2, nameof(Shutdown)), "{caller} is shutting down");

        private static readonly Action<ILogger, string, Exception> cancelRequest = LoggerMessage
            .Define<string>(LogLevel.Information, new (9, nameof(CancelRequest)), "{caller} is stopping early due to Cancellation Request");

        /// <summary>
        /// Logs a Startup event
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="caller"></param>
        internal static void Startup(this ILogger logger, string caller) => startup(logger, caller, null);

        /// <summary>
        /// Logs a Shutdown event
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="caller"></param>
        internal static void Shutdown(this ILogger logger, string caller) => shutdown(logger, caller, null);

        /// <summary>
        /// Logs a Task Cancellation event
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="caller"></param>
        internal static void CancelRequest(this ILogger logger, [CallerMemberName] string caller = null) => cancelRequest(logger, caller, null);
    }
}
