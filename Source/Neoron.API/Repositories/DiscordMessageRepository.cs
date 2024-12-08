using Microsoft.EntityFrameworkCore;
using Neoron.API.Data;
using Neoron.API.Interfaces;
using Neoron.API.Models;

namespace Neoron.API.Repositories;

public class DiscordMessageRepository : IDiscordMessageRepository
{
    private readonly ApplicationDbContext _context;

    public DiscordMessageRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<DiscordMessage>> GetAllAsync()
    {
        return await _context.DiscordMessages.ToListAsync();
    }

    public async Task<DiscordMessage?> GetByIdAsync(object id)
    {
        return await _context.DiscordMessages.FindAsync(id);
    }

    public async Task<DiscordMessage> AddAsync(DiscordMessage entity)
    {
        _context.DiscordMessages.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(DiscordMessage entity)
    {
        _context.Entry(entity).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(object id)
    {
        var message = await GetByIdAsync(id);
        if (message != null)
        {
            message.IsDeleted = true;
            message.DeletedAt = DateTimeOffset.UtcNow;
            await UpdateAsync(message);
        }
    }

    public async Task<IEnumerable<DiscordMessage>> GetByChannelIdAsync(long channelId)
    {
        return await _context.DiscordMessages
            .Where(m => m.ChannelId == channelId)
            .ToListAsync();
    }

    public async Task<IEnumerable<DiscordMessage>> GetByGuildIdAsync(long guildId)
    {
        return await _context.DiscordMessages
            .Where(m => m.GuildId == guildId)
            .ToListAsync();
    }

    public async Task<IEnumerable<DiscordMessage>> GetByAuthorIdAsync(long authorId)
    {
        return await _context.DiscordMessages
            .Where(m => m.AuthorId == authorId)
            .ToListAsync();
    }

    public async Task<IEnumerable<DiscordMessage>> GetThreadMessagesAsync(long threadId)
    {
        return await _context.DiscordMessages
            .Where(m => m.ThreadId == threadId)
            .ToListAsync();
    }
}
