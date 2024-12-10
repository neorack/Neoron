using Neoron.API.Models;

namespace Neoron.API.Interfaces
{
    /// <summary>
    /// Repository interface for managing staged Discord messages and attachments.
    /// </summary>
    public interface IStagingRepository
    {
        /// <summary>
        /// Stages a batch of Discord messages.
        /// </summary>
        Task<int> StageMessagesAsync(IEnumerable<DiscordMessage> messages);

        /// <summary>
        /// Gets pending staged messages for processing.
        /// </summary>
        Task<IEnumerable<StagedDiscordMessage>> GetPendingMessagesAsync(int batchSize);

        /// <summary>
        /// Updates the status of staged messages.
        /// </summary>
        Task UpdateMessageStatusAsync(IEnumerable<long> stagingIds, StagingStatus status, string? errorMessage = null);

        /// <summary>
        /// Stages file attachments for processing.
        /// </summary>
        Task<int> StageAttachmentsAsync(long messageId, IEnumerable<DiscordFileAttachment> files);

        /// <summary>
        /// Gets pending staged attachments for processing.
        /// </summary>
        Task<IEnumerable<DiscordFileAttachment>> GetPendingAttachmentsAsync(int batchSize);

        /// <summary>
        /// Updates the status of staged attachments.
        /// </summary>
        Task UpdateAttachmentStatusAsync(IEnumerable<long> attachmentIds, AttachmentStatus status, string? errorMessage = null);

        /// <summary>
        /// Checks for duplicate messages in staging.
        /// </summary>
        /// <param name="messageIds">Collection of message IDs to check</param>
        /// <returns>Collection of existing staging IDs</returns>
        Task<IEnumerable<long>> FindDuplicateMessagesAsync(IEnumerable<long> messageIds);
    }
}
