using Neoron.API.Models;

namespace Neoron.API.Interfaces
{
    public interface IDiscordLogIngestionService
    {
        /// <summary>
        /// Stages a batch of Discord messages for processing.
        /// </summary>
        /// <param name="messages">Collection of messages to stage</param>
        /// <param name="skipDuplicates">Whether to skip duplicate messages</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A tuple containing (number of staged messages, number of duplicates skipped)</returns>
        Task<(int staged, int duplicates)> StageMessagesAsync(IEnumerable<DiscordMessage> messages, bool skipDuplicates = true, CancellationToken cancellationToken = default);

        /// <summary>
        /// Processes staged messages and commits them to the database.
        /// </summary>
        /// <param name="batchSize">Optional batch size for processing</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of successfully processed messages</returns>
        Task<int> ProcessStagedMessagesAsync(int? batchSize = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Stages file attachments for processing.
        /// </summary>
        /// <param name="messageId">The ID of the message containing the attachments</param>
        /// <param name="files">Collection of file data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of successfully staged files</returns>
        Task<int> StageAttachmentsAsync(long messageId, IEnumerable<DiscordFileAttachment> files, CancellationToken cancellationToken = default);

        /// <summary>
        /// Processes staged file attachments and stores them in FileTable.
        /// </summary>
        /// <param name="batchSize">Optional batch size for processing</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of successfully processed files</returns>
        Task<int> ProcessStagedAttachmentsAsync(int? batchSize = null, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets the current rate limit status
        /// </summary>
        /// <returns>True if currently rate limited, false otherwise</returns>
        bool IsRateLimited();

        /// <summary>
        /// Synchronizes messages from a specific checkpoint.
        /// </summary>
        /// <param name="guildId">The guild ID to sync</param>
        /// <param name="channelId">The channel ID to sync</param>
        /// <param name="fromMessageId">Optional message ID to start from</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of messages synchronized</returns>
        Task<int> SyncMessagesAsync(long guildId, long channelId, long? fromMessageId = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the latest sync checkpoint for a channel.
        /// </summary>
        /// <param name="guildId">The guild ID</param>
        /// <param name="channelId">The channel ID</param>
        /// <returns>The sync checkpoint or null if none exists</returns>
        Task<SyncCheckpoint?> GetSyncCheckpointAsync(long guildId, long channelId);
    }
}
