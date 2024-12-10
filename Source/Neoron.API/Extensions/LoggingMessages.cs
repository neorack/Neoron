using System;
using Microsoft.Extensions.Logging;
using Serilog.Events;
using LoggerMessage = Microsoft.Extensions.Logging.LoggerMessage;

namespace Neoron.API.Extensions
{
    /// <summary>
    /// Provides logging messages for the application.
    /// </summary>
    public static class LoggingMessages
    {
        private static readonly Action<ILogger, string?, Exception?> FailedToAddMessageDelegate;

        static LoggingMessages()
        {
            FailedToAddMessageDelegate = LoggerMessage.Define<string?>(
                LogLevel.Warning,
                new EventId(1, nameof(FailedToAddMessage)),
                "Failed to add message for client {ClientId}");
        }

        /// <summary>
        /// Logs a warning message indicating a failure to add a message.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="clientId">The client ID.</param>
        /// <param name="ex">The exception that occurred.</param>
        public static void FailedToAddMessage(ILogger logger, string? clientId, Exception ex)
        {
            FailedToAddMessageDelegate(logger, clientId, ex);
        }
    }
}
