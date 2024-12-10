using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Neoron.API.Data;
using Neoron.API.Interfaces;
using Neoron.API.Logging;
using Neoron.API.Models;

namespace Neoron.API.Repositories
{
    /// <summary>
    /// Repository for managing Discord messages.
    /// </summary>
    public class DiscordMessageRepository : IDiscordMessageRepository
    {
        private readonly ApplicationDbContext context;
        private readonly ILogger<DiscordMessageRepository> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscordMessageRepository"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="logger">The logger instance.</param>
        public DiscordMessageRepository(ApplicationDbContext context, ILogger<DiscordMessageRepository> logger)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(logger);

            this.context = context;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<DiscordMessage>> GetAllAsync()
        {
            return await context.DiscordMessages.ToListAsync().ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<DiscordMessage?> GetByIdAsync(object id)
        {
            return await context.DiscordMessages.FindAsync(id).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<DiscordMessage> AddAsync(DiscordMessage entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            using var activity = Activity.Current?.Source.StartActivity("AddMessage");
            try
            {
                LogMessages.LogAddingMessage(logger, entity.MessageId, entity.ChannelId);

                context.DiscordMessages.Add(entity);
                await context.SaveChangesAsync().ConfigureAwait(false);

                LogMessages.LogAddedMessage(logger, entity.MessageId);
                return entity;
            }
            catch (Exception ex)
            {
                LogMessages.LogAddMessageError(logger, ex, entity.MessageId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task UpdateAsync(DiscordMessage entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            LogMessages.LogUpdatingMessage(logger, entity.MessageId);

            const int maxRetries = 3;
            var retryCount = 0;

            while (retryCount < maxRetries)
            {
                try
                {
                    context.Entry(entity).State = EntityState.Modified;
                    await context.SaveChangesAsync().ConfigureAwait(false);
                    LogMessages.LogUpdatedMessage(logger, entity.MessageId);
                    return;
                }
                catch (DbUpdateConcurrencyException) when (retryCount < maxRetries - 1)
                {
                    LogMessages.LogUpdateRetry(logger, entity.MessageId);
                    await Task.Delay(100 * (retryCount + 1)).ConfigureAwait(false);
                    retryCount++;
                    await context.Entry(entity).ReloadAsync().ConfigureAwait(false);
                }
            }

            LogMessages.LogUpdateError(logger, entity.MessageId);
            throw new DbUpdateConcurrencyException("Failed to update after maximum retries");
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(object id)
        {
            try
            {
                var message = await GetByIdAsync(id).ConfigureAwait(false);
                if (message != null)
                {
                    LogMessages.LogDeletingMessage(logger, message.MessageId);
                    message.IsDeleted = true;
                    message.DeletedAt = DateTimeOffset.UtcNow;
                    await UpdateAsync(message).ConfigureAwait(false);
                    LogMessages.LogDeletedMessage(logger, message.MessageId);
                }
            }
            catch (Exception ex)
            {
                LogMessages.LogDeleteError(logger, ex, id);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<DiscordMessage>> GetByChannelIdAsync(long channelId, int skip = 0, int take = 100)
        {
            return await context.DiscordMessages
                .Where(m => m.ChannelId == channelId)
                .OrderByDescending(m => m.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync()
                .ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<DiscordMessage>> GetByGuildIdAsync(long guildId, int skip = 0, int take = 100)
        {
            return await context.DiscordMessages
                .Where(m => m.GuildId == guildId)
                .OrderByDescending(m => m.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync()
                .ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<DiscordMessage>> GetByAuthorIdAsync(long authorId, int skip = 0, int take = 100)
        {
            return await context.DiscordMessages
                .Where(m => m.AuthorId == authorId)
                .OrderByDescending(m => m.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync()
                .ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<DiscordMessage>> GetThreadMessagesAsync(long threadId, int skip = 0, int take = 100)
        {
            return await context.DiscordMessages
                .Where(m => m.ThreadId == threadId)
                .OrderByDescending(m => m.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync()
                .ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task<int> AddRangeAsync(IEnumerable<DiscordMessage> messages)
        {
            ArgumentNullException.ThrowIfNull(messages);

            await using var transaction = await context.Database.BeginTransactionAsync().ConfigureAwait(false);
            try
            {
                LogMessages.LogAddingRange(logger);

                await context.DiscordMessages.AddRangeAsync(messages).ConfigureAwait(false);
                var result = await context.SaveChangesAsync().ConfigureAwait(false);
                await transaction.CommitAsync().ConfigureAwait(false);

                LogMessages.LogAddedRange(logger);
                return result;
            }
            catch (Exception ex)
            {
                LogMessages.LogAddRangeError(logger, ex);
                await transaction.RollbackAsync().ConfigureAwait(false);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<int> UpdateRangeAsync(IEnumerable<DiscordMessage> messages)
        {
            ArgumentNullException.ThrowIfNull(messages);

            await using var transaction = await context.Database.BeginTransactionAsync().ConfigureAwait(false);
            try
            {
                foreach (var message in messages)
                {
                    context.Entry(message).State = EntityState.Modified;
                }

                var result = await context.SaveChangesAsync().ConfigureAwait(false);
                await transaction.CommitAsync().ConfigureAwait(false);

                return result;
            }
            catch
            {
                await transaction.RollbackAsync().ConfigureAwait(false);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<int> DeleteRangeAsync(IEnumerable<long> messageIds)
        {
            ArgumentNullException.ThrowIfNull(messageIds);

            await using var transaction = await context.Database.BeginTransactionAsync().ConfigureAwait(false);
            try
            {
                var messages = await context.DiscordMessages
                    .Where(m => messageIds.Contains(m.MessageId))
                    .ToListAsync()
                    .ConfigureAwait(false);

                foreach (var message in messages)
                {
                    message.IsDeleted = true;
                    message.DeletedAt = DateTimeOffset.UtcNow;
                    context.Entry(message).State = EntityState.Modified;
                }

                var result = await context.SaveChangesAsync().ConfigureAwait(false);
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
