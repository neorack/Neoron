namespace Neoron.API.Logging
{
    /// <summary>
    /// Contains structured logging message definitions for Discord message operations.
    /// </summary>
    public static partial class LogMessages
    {
        /// <summary>
        /// Logs when a message is being added.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="messageId">The ID of the message being added.</param>
        /// <param name="channelId">The ID of the channel where the message is being added.</param>
        [LoggerMessage(Level = LogLevel.Information, Message = "Adding message {MessageId} for channel {ChannelId}")]
        public static partial void LogAddingMessage(ILogger logger, long messageId, long channelId);

        /// <summary>
        /// Logs when a message has been successfully added.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="messageId">The ID of the message that was added.</param>
        [LoggerMessage(Level = LogLevel.Information, Message = "Successfully added message {MessageId}")]
        public static partial void LogAddedMessage(ILogger logger, long messageId);

        /// <summary>
        /// Logs when adding a message fails.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="ex">The exception that occurred.</param>
        /// <param name="messageId">The ID of the message that failed to add.</param>
        [LoggerMessage(Level = LogLevel.Error, Message = "Failed to add message {MessageId}")]
        public static partial void LogAddMessageError(ILogger logger, Exception ex, long messageId);

        /// <summary>
        /// Logs when a message is being updated.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="messageId">The ID of the message being updated.</param>
        [LoggerMessage(Level = LogLevel.Information, Message = "Updating message {MessageId}")]
        public static partial void LogUpdatingMessage(ILogger logger, long messageId);

        /// <summary>
        /// Logs when a message has been successfully updated.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="messageId">The ID of the message that was updated.</param>
        [LoggerMessage(Level = LogLevel.Information, Message = "Successfully updated message {MessageId}")]
        public static partial void LogUpdatedMessage(ILogger logger, long messageId);

        /// <summary>
        /// Logs when a concurrency conflict occurs during message update.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="messageId">The ID of the message being updated.</param>
        [LoggerMessage(Level = LogLevel.Warning, Message = "Concurrency conflict when updating message {MessageId}, retrying...")]
        public static partial void LogUpdateRetry(ILogger logger, long messageId);

        /// <summary>
        /// Logs when message update fails after maximum retries.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="messageId">The ID of the message that failed to update.</param>
        [LoggerMessage(Level = LogLevel.Error, Message = "Failed to update message {MessageId} after maximum retries")]
        public static partial void LogUpdateError(ILogger logger, long messageId);

        /// <summary>
        /// Logs when a message is being deleted.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="messageId">The ID of the message being deleted.</param>
        [LoggerMessage(Level = LogLevel.Information, Message = "Deleting message {MessageId}")]
        public static partial void LogDeletingMessage(ILogger logger, long messageId);

        /// <summary>
        /// Logs when a message has been successfully deleted.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="messageId">The ID of the message that was deleted.</param>
        [LoggerMessage(Level = LogLevel.Information, Message = "Successfully deleted message {MessageId}")]
        public static partial void LogDeletedMessage(ILogger logger, long messageId);

        /// <summary>
        /// Logs when deleting a message fails.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="ex">The exception that occurred.</param>
        /// <param name="id">The ID of the message that failed to delete.</param>
        [LoggerMessage(Level = LogLevel.Error, Message = "Failed to delete message with id {Id}")]
        public static partial void LogDeleteError(ILogger logger, Exception ex, object id);

        /// <summary>
        /// Logs when a range of messages is being added.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        [LoggerMessage(Level = LogLevel.Information, Message = "Adding a range of messages")]
        public static partial void LogAddingRange(ILogger logger);

        /// <summary>
        /// Logs when a range of messages has been successfully added.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        [LoggerMessage(Level = LogLevel.Information, Message = "Successfully added a range of messages")]
        public static partial void LogAddedRange(ILogger logger);

        /// <summary>
        /// Logs when adding a range of messages fails.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="ex">The exception that occurred.</param>
        [LoggerMessage(Level = LogLevel.Error, Message = "Failed to add a range of messages")]
        public static partial void LogAddRangeError(ILogger logger, Exception ex);
    }
}
