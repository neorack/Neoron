namespace Neoron.API.Interfaces;

public interface IDiscordMessageRepository : IRepository<DiscordMessage>
{
    Task<IEnumerable<DiscordMessage>> GetByChannelIdAsync(long channelId, int skip = 0, int take = 100);
    Task<IEnumerable<DiscordMessage>> GetByGuildIdAsync(long guildId, int skip = 0, int take = 100);
    Task<IEnumerable<DiscordMessage>> GetByAuthorIdAsync(long authorId, int skip = 0, int take = 100);
    Task<IEnumerable<DiscordMessage>> GetThreadMessagesAsync(long threadId, int skip = 0, int take = 100);
    Task<int> AddRangeAsync(IEnumerable<DiscordMessage> messages);
    Task<int> UpdateRangeAsync(IEnumerable<DiscordMessage> messages);
    Task<int> DeleteRangeAsync(IEnumerable<long> messageIds);
}
