namespace Neoron.API.Interfaces;

public interface IDiscordMessageRepository : IRepository<DiscordMessage>
{
    Task<IEnumerable<DiscordMessage>> GetByChannelIdAsync(long channelId);
    Task<IEnumerable<DiscordMessage>> GetByGuildIdAsync(long guildId);
    Task<IEnumerable<DiscordMessage>> GetByAuthorIdAsync(long authorId);
    Task<IEnumerable<DiscordMessage>> GetThreadMessagesAsync(long threadId);
}
