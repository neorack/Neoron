using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Neoron.API.Data;
using Neoron.API.Interfaces;
using Neoron.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Neoron.API.Repositories
{
    public class DiscordMessageRepository : IDiscordMessageRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DiscordMessageRepository> _logger;

        public DiscordMessageRepository(
            ApplicationDbContext context,
            ILogger<DiscordMessageRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<DiscordMessage>> GetAllAsync() =>
            await _context.DiscordMessages.ToListAsync().ConfigureAwait(false);

        public async Task<DiscordMessage?> GetByIdAsync(object id) =>
            await _context.DiscordMessages.FindAsync(id).ConfigureAwait(false);

        public async Task<DiscordMessage> AddAsync(DiscordMessage entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

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
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            _logger.LogInformation("Updating message {MessageId}", entity.MessageId);

            const int maxRetries = 3;
            var retryCount = 0;

            while (retryCount < maxRetries)
            {
                try
                {
                    _context.Entry(entity).State = EntityState.Modified;
                    await _context.SaveChangesAsync().ConfigureAwait(false);
                    _logger.LogInformation("Successfully updated message {MessageId}", entity.MessageId);
                    return;
                }
                catch (DbUpdateConcurrencyException) when (retryCount < maxRetries - 1)
                {
                    _logger.LogWarning("Concurrency conflict when updating message {MessageId}, retrying...", entity.MessageId);
                    await Task.Delay(100 * (retryCount + 1)).ConfigureAwait(false);
                    retryCount++;
                    await _context.Entry(entity).ReloadAsync().ConfigureAwait(false);
                }
            }

            _logger.LogError("Failed to update message {MessageId} after maximum retries", entity.MessageId);
            throw new DbUpdateConcurrencyException("Failed to update after maximum retries");
        }

        public async Task DeleteAsync(object id)
        {
            try
            {
                var message = await GetByIdAsync(id).ConfigureAwait(false);
                if (message != null)
                {
                    _logger.LogInformation("Deleting message {MessageId}", message.MessageId);
                    message.IsDeleted = true;
                    message.DeletedAt = DateTimeOffset.UtcNow;
                    await UpdateAsync(message).ConfigureAwait(false);
                    _logger.LogInformation("Successfully deleted message {MessageId}", message.MessageId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete message with id {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<DiscordMessage>> GetByChannelIdAsync(long channelId, int skip = 0, int take = 100) =>
            await _context.DiscordMessages
                .Where(m => m.ChannelId == channelId)
                .OrderByDescending(m => m.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync().ConfigureAwait(false);

        public async Task<IEnumerable<DiscordMessage>> GetByGuildIdAsync(long guildId, int skip = 0, int take = 100) =>
            await _context.DiscordMessages
                .Where(m => m.GuildId == guildId)
                .OrderByDescending(m => m.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync().ConfigureAwait(false);

        public async Task<IEnumerable<DiscordMessage>> GetByAuthorIdAsync(long authorId, int skip = 0, int take = 100) =>
            await _context.DiscordMessages
                .Where(m => m.AuthorId == authorId)
                .OrderByDescending(m => m.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync().ConfigureAwait(false);

        public async Task<IEnumerable<DiscordMessage>> GetThreadMessagesAsync(long threadId, int skip = 0, int take = 100) =>
            await _context.DiscordMessages
                .Where(m => m.ThreadId == threadId)
                .OrderByDescending(m => m.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync().ConfigureAwait(false);

        public async Task<int> AddRangeAsync(IEnumerable<DiscordMessage> messages)
        {
            if (messages == null)
            {
                throw new ArgumentNullException(nameof(messages));
            }

            await using var transaction = await _context.Database.BeginTransactionAsync().ConfigureAwait(false);
            try
            {
                _logger.LogInformation("Adding a range of messages");
                await _context.DiscordMessages.AddRangeAsync(messages).ConfigureAwait(false);
                var result = await _context.SaveChangesAsync().ConfigureAwait(false);
                await transaction.CommitAsync().ConfigureAwait(false);
                _logger.LogInformation("Successfully added a range of messages");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to add a range of messages");
                await transaction.RollbackAsync().ConfigureAwait(false);
                throw;
            }
        }

        public async Task<int> UpdateRangeAsync(IEnumerable<DiscordMessage> messages)
        {
            if (messages == null)
            {
                throw new ArgumentNullException(nameof(messages));
            }

            await using var transaction = await _context.Database.BeginTransactionAsync().ConfigureAwait(false);
            try
            {
                foreach (var message in messages)
                {
                    _context.Entry(message).State = EntityState.Modified;
                }
                var result = await _context.SaveChangesAsync().ConfigureAwait(false);
                await transaction.CommitAsync().ConfigureAwait(false);
                return result;
            }
            catch
            {
                await transaction.RollbackAsync().ConfigureAwait(false);
                throw;
            }
        }

        public async Task<int> DeleteRangeAsync(IEnumerable<long> messageIds)
        {
            if (messageIds == null)
            {
                throw new ArgumentNullException(nameof(messageIds));
            }

            await using var transaction = await _context.Database.BeginTransactionAsync().ConfigureAwait(false);
            try
            {
                var messages = await _context.DiscordMessages
                    .Where(m => messageIds.Contains(m.MessageId))
                    .ToListAsync().ConfigureAwait(false);

                foreach (var message in messages)
                {
                    message.IsDeleted = true;
                    message.DeletedAt = DateTimeOffset.UtcNow;
                    _context.Entry(message).State = EntityState.Modified;
                }

                var result = await _context.SaveChangesAsync().ConfigureAwait(false);
                await transaction.CommitAsync().ConfigureAwait(false);
                return result;
            }
            catch
            {
                await transaction.RollbackAsync().ConfigureAwait(false);
                throw;
            }
        }
    }
}
