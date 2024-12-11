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
        /// Bulk updates the group assignments for multiple messages.
        /// </summary>
        /// <param name="messageIds">Collection of message IDs to update (must not be null)</param>
        /// <param name="groupId">Target group ID to assign (use 0 to remove from groups)</param>
        /// <returns>Number of messages successfully updated</returns>
        /// <remarks>
        /// Provides atomic group assignment with:
        /// - Validation of message and group existence
        /// - Maintenance of group constraints and limits
        /// - Automatic audit logging of changes
        /// - Support for batch operations
        /// </remarks>
        /// <exception cref="ArgumentNullException">When messageIds is null</exception>
        /// <exception cref="ArgumentException">When groupId is invalid</exception>
        /// <exception cref="InvalidOperationException">When batch update fails</exception>
        Task<int> UpdateGroupAssignmentAsync(IEnumerable<long> messageIds, long groupId);

        /// <summary>
        /// Retrieves all channel groups associated with a specific guild.
        /// </summary>
        /// <param name="guildId">The Discord guild identifier</param>
        /// <returns>Collection of channel groups with their relationships loaded</returns>
        /// <remarks>
        /// Includes:
        /// - Group metadata and settings
        /// - Channel associations
        /// - Member permissions
        /// - Usage statistics
        /// Results are cached for performance
        /// </remarks>
        /// <exception cref="ArgumentException">When guildId is invalid</exception>
        /// <exception cref="InvalidOperationException">When guild access fails</exception>
        Task<IEnumerable<ChannelGroup>> GetChannelGroupsAsync(long guildId);

        /// <summary>
        /// Creates a new channel group with specified configuration.
        /// </summary>
        /// <param name="group">Channel group configuration (must not be null)</param>
        /// <returns>The newly created channel group with generated ID</returns>
        /// <remarks>
        /// Enforces:
        /// - Group naming conventions
        /// - Permission requirements
        /// - Channel limit validations
        /// - Duplicate detection
        /// Triggers audit logging
        /// </remarks>
        /// <exception cref="ArgumentNullException">When group is null</exception>
        /// <exception cref="ValidationException">When group configuration is invalid</exception>
        /// <exception cref="InvalidOperationException">When creation fails</exception>
        Task<ChannelGroup> CreateChannelGroupAsync(ChannelGroup group);

        /// <summary>
        /// Updates configuration and settings for an existing channel group.
        /// </summary>
        /// <param name="group">Updated group configuration (must not be null)</param>
        /// <returns>True if group was found and updated, false if not found</returns>
        /// <remarks>
        /// Supports:
        /// - Partial updates of properties
        /// - Permission validation
        /// - Optimistic concurrency
        /// - Audit history tracking
        /// Changes are atomic
        /// </remarks>
        /// <exception cref="ArgumentNullException">When group is null</exception>
        /// <exception cref="ValidationException">When updated configuration is invalid</exception>
        /// <exception cref="ConcurrencyException">When version conflict occurs</exception>
        Task<bool> UpdateChannelGroupAsync(ChannelGroup group);

        /// <summary>
        /// Performs a soft delete of a channel group.
        /// </summary>
        /// <param name="groupId">The group identifier to delete</param>
        /// <returns>True if group was found and deleted, false if not found</returns>
        /// <remarks>
        /// Implements soft delete:
        /// - Marks group as inactive
        /// - Preserves message associations
        /// - Updates related statistics
        /// - Maintains audit history
        /// Deletion is reversible
        /// </remarks>
        /// <exception cref="ArgumentException">When groupId is invalid</exception>
        /// <exception cref="InvalidOperationException">When deletion fails</exception>
        Task<bool> DeleteChannelGroupAsync(long groupId);
    }
}
