using Neoron.API.Models;

namespace Neoron.API.Interfaces
{
    /// <summary>
    /// Repository interface for managing Discord messages.
    /// </summary>
    public interface IDiscordMessageRepository : IRepository<DiscordMessage>
    {
        /// <summary>
        /// Gets Discord messages by channel identifier asynchronously.
        /// </summary>
        /// <param name="channelId">The channel identifier.</param>
        /// <param name="skip">The number of messages to skip.</param>
        /// <param name="take">The number of messages to take.</param>
        /// <returns>A collection of Discord messages.</returns>
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
    }
}
