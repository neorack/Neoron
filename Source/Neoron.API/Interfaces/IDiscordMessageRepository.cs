using Neoron.API.Models;

namespace Neoron.API.Interfaces
{
    /// <summary>
    /// Repository interface for managing Discord messages.
    /// </summary>
    public interface IDiscordMessageRepository : IRepository<DiscordMessage>
    {
        /// <summary>
        /// Retrieves paginated Discord messages for a specific channel.
        /// </summary>
        /// <remarks>
        /// Messages are ordered by creation date descending.
        /// Supports efficient pagination for large channels.
        /// Includes soft-deleted messages if configured.
        /// </remarks>
        /// <param name="channelId">The Discord channel identifier</param>
        /// <param name="skip">Number of messages to skip for pagination (must be >= 0)</param>
        /// <param name="take">Page size (must be between 1 and 1000 for performance)</param>
        /// <returns>Collection of messages with their relationships loaded</returns>
        /// <exception cref="ArgumentException">When skip or take parameters are invalid</exception>
        /// <exception cref="InvalidOperationException">When channel access fails</exception>
        Task<IEnumerable<DiscordMessage>> GetByChannelIdAsync(long channelId, int skip = 0, int take = 100);

        /// <summary>
        /// Gets Discord messages by guild identifier asynchronously.
        /// </summary>
        /// <param name="guildId">The guild identifier.</param>
        /// <param name="skip">The number of messages to skip.</param>
        /// <param name="take">The number of messages to take.</param>
        /// <returns>A collection of Discord messages.</returns>
        Task<IEnumerable<DiscordMessage>> GetByGuildIdAsync(long guildId, int skip = 0, int take = 100);

        /// <summary>
        /// Gets Discord messages by author identifier asynchronously.
        /// </summary>
        /// <param name="authorId">The author identifier.</param>
        /// <param name="skip">The number of messages to skip.</param>
        /// <param name="take">The number of messages to take.</param>
        /// <returns>A collection of Discord messages.</returns>
        Task<IEnumerable<DiscordMessage>> GetByAuthorIdAsync(long authorId, int skip = 0, int take = 100);

        /// <summary>
        /// Retrieves paginated messages within a specific thread.
        /// </summary>
        /// <param name="threadId">The Discord thread identifier</param>
        /// <param name="skip">Number of messages to skip (must be >= 0)</param>
        /// <param name="take">Number of messages to retrieve (1-1000)</param>
        /// <returns>Collection of messages in thread order</returns>
        /// <remarks>
        /// Messages are ordered by position in thread.
        /// Includes thread metadata and relationships.
        /// Supports efficient pagination for long threads.
        /// </remarks>
        /// <exception cref="ArgumentException">When skip or take parameters are invalid</exception>
        /// <exception cref="InvalidOperationException">When thread access fails</exception>
        Task<IEnumerable<DiscordMessage>> GetThreadMessagesAsync(long threadId, int skip = 0, int take = 100);

        /// <summary>
        /// Adds multiple Discord messages in a single transaction.
        /// </summary>
        /// <param name="messages">Collection of messages to add</param>
        /// <returns>Number of messages successfully added</returns>
        /// <remarks>
        /// Performs bulk insert for better performance.
        /// Maintains message relationships and integrity.
        /// Handles duplicate detection.
        /// </remarks>
        /// <exception cref="ArgumentNullException">When messages collection is null</exception>
        /// <exception cref="InvalidOperationException">When bulk insert fails</exception>
        Task<int> AddRangeAsync(IEnumerable<DiscordMessage> messages);

        /// <summary>
        /// Updates multiple Discord messages in a single transaction.
        /// </summary>
        /// <param name="messages">Collection of messages to update</param>
        /// <returns>Number of messages successfully updated</returns>
        /// <remarks>
        /// Provides atomic updates with:
        /// - Optimistic concurrency using Version property
        /// - Automatic audit history tracking
        /// - Relationship consistency maintenance
        /// - Support for partial updates
        /// - Batch size limits for performance
        /// </remarks>
        /// <exception cref="ArgumentNullException">When messages collection is null</exception>
        /// <exception cref="ConcurrencyException">When version conflicts occur</exception>
        /// <exception cref="InvalidOperationException">When bulk update fails</exception>
        Task<int> UpdateRangeAsync(IEnumerable<DiscordMessage> messages);

        /// <summary>
        /// Performs soft deletion of multiple messages in a single operation.
        /// </summary>
        /// <param name="messageIds">Collection of message identifiers to delete</param>
        /// <returns>Number of messages successfully marked as deleted</returns>
        /// <remarks>
        /// Implements soft delete pattern:
        /// - Sets IsDeleted flag to true
        /// - Updates DeletedAt timestamp
        /// - Maintains referential integrity
        /// - Preserves message history
        /// </remarks>
        /// <exception cref="ArgumentNullException">When messageIds is null</exception>
        /// <exception cref="InvalidOperationException">When deletion fails</exception>
        Task<int> DeleteRangeAsync(IEnumerable<long> messageIds);

        /// <summary>
        /// Identifies which messages from a collection already exist in storage.
        /// </summary>
        /// <param name="messageIds">Collection of message IDs to check</param>
        /// <returns>Collection of existing message IDs</returns>
        /// <remarks>
        /// Used for:
        /// - Duplicate detection during imports
        /// - Sync validation
        /// - Integrity checks
        /// Optimized for bulk operations
        /// </remarks>
        /// <exception cref="ArgumentNullException">When messageIds is null</exception>
        Task<IEnumerable<long>> FindExistingMessagesAsync(IEnumerable<long> messageIds);

        /// <summary>
        /// Retrieves paginated messages belonging to a specific group.
        /// </summary>
        /// <param name="groupId">The group identifier</param>
        /// <param name="skip">Number of messages to skip (must be >= 0)</param>
        /// <param name="take">Number of messages to retrieve (1-1000)</param>
        /// <returns>Collection of messages in the group</returns>
        /// <remarks>
        /// Supports organization features:
        /// - Custom message grouping
        /// - Efficient pagination
        /// - Ordered by group-specific criteria
        /// - Includes group metadata
        /// </remarks>
        /// <exception cref="ArgumentException">When pagination parameters are invalid</exception>
        /// <exception cref="InvalidOperationException">When group access fails</exception>
        Task<IEnumerable<DiscordMessage>> GetByGroupIdAsync(long groupId, int skip = 0, int take = 100);

        /// <summary>
        /// Updates group assignments for messages.
        /// </summary>
        /// <param name="messageIds">The message IDs to update.</param>
        /// <param name="groupId">The group ID to assign.</param>
        /// <returns>The number of messages updated.</returns>
        Task<int> UpdateGroupAssignmentAsync(IEnumerable<long> messageIds, long groupId);

        /// <summary>
        /// Gets channel groups for a guild.
        /// </summary>
        /// <param name="guildId">The guild ID.</param>
        /// <returns>Collection of channel groups.</returns>
        Task<IEnumerable<ChannelGroup>> GetChannelGroupsAsync(long guildId);

        /// <summary>
        /// Creates a new channel group.
        /// </summary>
        /// <param name="group">The group to create.</param>
        /// <returns>The created group.</returns>
        Task<ChannelGroup> CreateChannelGroupAsync(ChannelGroup group);

        /// <summary>
        /// Updates an existing channel group.
        /// </summary>
        /// <param name="group">The group to update.</param>
        /// <returns>True if updated, false if not found.</returns>
        Task<bool> UpdateChannelGroupAsync(ChannelGroup group);

        /// <summary>
        /// Deletes a channel group.
        /// </summary>
        /// <param name="groupId">The group ID to delete.</param>
        /// <returns>True if deleted, false if not found.</returns>
        Task<bool> DeleteChannelGroupAsync(long groupId);
    }
}
