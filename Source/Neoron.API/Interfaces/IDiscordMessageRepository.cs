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
        /// Gets thread messages by thread identifier asynchronously.
        /// </summary>
        /// <param name="threadId">The thread identifier.</param>
        /// <param name="skip">The number of messages to skip.</param>
        /// <param name="take">The number of messages to take.</param>
        /// <returns>A collection of Discord messages.</returns>
        Task<IEnumerable<DiscordMessage>> GetThreadMessagesAsync(long threadId, int skip = 0, int take = 100);

        /// <summary>
        /// Adds a range of Discord messages asynchronously.
        /// </summary>
        /// <param name="messages">The collection of messages to add.</param>
        /// <returns>The number of messages added.</returns>
        Task<int> AddRangeAsync(IEnumerable<DiscordMessage> messages);

        /// <summary>
        /// Updates a range of Discord messages asynchronously.
        /// </summary>
        /// <param name="messages">The collection of messages to update.</param>
        /// <returns>The number of messages updated.</returns>
        Task<int> UpdateRangeAsync(IEnumerable<DiscordMessage> messages);

        /// <summary>
        /// Deletes a range of Discord messages by their identifiers asynchronously.
        /// </summary>
        /// <param name="messageIds">The collection of message identifiers to delete.</param>
        /// <returns>The number of messages deleted.</returns>
        Task<int> DeleteRangeAsync(IEnumerable<long> messageIds);

        /// <summary>
        /// Checks if messages already exist in the repository.
        /// </summary>
        /// <param name="messageIds">Collection of message IDs to check</param>
        /// <returns>Collection of existing message IDs</returns>
        Task<IEnumerable<long>> FindExistingMessagesAsync(IEnumerable<long> messageIds);

        /// <summary>
        /// Gets messages by group identifier asynchronously.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="skip">The number of messages to skip.</param>
        /// <param name="take">The number of messages to take.</param>
        /// <returns>A collection of Discord messages.</returns>
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
