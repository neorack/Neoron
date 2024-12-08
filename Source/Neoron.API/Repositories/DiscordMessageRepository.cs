using Microsoft.EntityFrameworkCore;
using Neoron.API.Data;
using Neoron.API.Interfaces;
using Neoron.API.Models;

namespace Neoron.API.Repositories;

public class DiscordMessageRepository : IDiscordMessageRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DiscordMessageRepository> _logger;

    public DiscordMessageRepository(
        ApplicationDbContext context,
        ILogger<DiscordMessageRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<DiscordMessage>> GetAllAsync()
    {
        return await _context.DiscordMessages.ToListAsync().ConfigureAwait(false);
    }

    public async Task<DiscordMessage?> GetByIdAsync(object id)
    {
        return await _context.DiscordMessages.FindAsync(id);
    }

    public async Task<DiscordMessage> AddAsync(DiscordMessage entity)
    {
        using var activity = System.Diagnostics.Activity.Current?.Source.StartActivity("AddMessage");
        try
        {
            _logger.LogInformation("Adding message {MessageId} for channel {ChannelId}", 
                entity.MessageId, entity.ChannelId);
            
            _context.DiscordMessages.Add(entity);
            await _context.SaveChangesAsync().ConfigureAwait(false);
            
            _logger.LogInformation("Successfully added message {MessageId}", entity.MessageId);
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add message {MessageId}", entity.MessageId);
            throw;
        }
    }

    public async Task UpdateAsync(DiscordMessage entity)
    {
        const int maxRetries = 3;
        var retryCount = 0;

        while (retryCount < maxRetries)
        {
            try
            {
                _context.Entry(entity).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return;
            }
            catch (DbUpdateConcurrencyException) when (retryCount < maxRetries - 1)
            {
                await Task.Delay(100 * (retryCount + 1)); // Exponential backoff
                retryCount++;
                
                // Reload the entity and retry
                await _context.Entry(entity).ReloadAsync();
            }
        }
        
        // If we get here, we've exhausted our retries
        throw new DbUpdateConcurrencyException("Failed to update after maximum retries");
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

    public async Task<IEnumerable<DiscordMessage>> GetByChannelIdAsync(long channelId, int skip = 0, int take = 100)
    {
        return await _context.DiscordMessages
            .Where(m => m.ChannelId == channelId)
            .OrderByDescending(m => m.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<IEnumerable<DiscordMessage>> GetByGuildIdAsync(long guildId, int skip = 0, int take = 100)
    {
        return await _context.DiscordMessages
            .Where(m => m.GuildId == guildId)
            .OrderByDescending(m => m.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<IEnumerable<DiscordMessage>> GetByAuthorIdAsync(long authorId, int skip = 0, int take = 100)
    {
        return await _context.DiscordMessages
            .Where(m => m.AuthorId == authorId)
            .OrderByDescending(m => m.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<IEnumerable<DiscordMessage>> GetThreadMessagesAsync(long threadId, int skip = 0, int take = 100)
    {
        return await _context.DiscordMessages
            .Where(m => m.ThreadId == threadId)
            .OrderByDescending(m => m.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<int> AddRangeAsync(IEnumerable<DiscordMessage> messages)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            await _context.DiscordMessages.AddRangeAsync(messages);
            var result = await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return result;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<int> UpdateRangeAsync(IEnumerable<DiscordMessage> messages)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            foreach (var message in messages)
            {
                _context.Entry(message).State = EntityState.Modified;
            }
            var result = await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return result;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<int> DeleteRangeAsync(IEnumerable<long> messageIds)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var messages = await _context.DiscordMessages
                .Where(m => messageIds.Contains(m.MessageId))
                .ToListAsync();

            foreach (var message in messages)
            {
                message.IsDeleted = true;
                message.DeletedAt = DateTimeOffset.UtcNow;
                _context.Entry(message).State = EntityState.Modified;
            }

            var result = await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return result;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
