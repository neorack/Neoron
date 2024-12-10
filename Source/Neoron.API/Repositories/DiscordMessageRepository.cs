using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Neoron.API.Data;
using Neoron.API.Interfaces;
using Neoron.API.Models;

    public static partial class LogMessages
    {
        [LoggerMessage(Level = LogLevel.Information, Message = "Adding message {MessageId} for channel {ChannelId}")]
        public static partial void LogAddingMessage(ILogger logger, long messageId, long channelId);

        [LoggerMessage(Level = LogLevel.Information, Message = "Successfully added message {MessageId}")]
        public static partial void LogAddedMessage(ILogger logger, long messageId);

        [LoggerMessage(Level = LogLevel.Error, Message = "Failed to add message {MessageId}")]
        public static partial void LogAddMessageError(ILogger logger, Exception ex, long messageId);

        [LoggerMessage(Level = LogLevel.Information, Message = "Updating message {MessageId}")]
        public static partial void LogUpdatingMessage(ILogger logger, long messageId);

        [LoggerMessage(Level = LogLevel.Information, Message = "Successfully updated message {MessageId}")]
        public static partial void LogUpdatedMessage(ILogger logger, long messageId);

        [LoggerMessage(Level = LogLevel.Warning, Message = "Concurrency conflict when updating message {MessageId}, retrying...")]
        public static partial void LogUpdateRetry(ILogger logger, long messageId);

        [LoggerMessage(Level = LogLevel.Error, Message = "Failed to update message {MessageId} after maximum retries")]
        public static partial void LogUpdateError(ILogger logger, long messageId);

        [LoggerMessage(Level = LogLevel.Information, Message = "Deleting message {MessageId}")]
        public static partial void LogDeletingMessage(ILogger logger, long messageId);

        [LoggerMessage(Level = LogLevel.Information, Message = "Successfully deleted message {MessageId}")]
        public static partial void LogDeletedMessage(ILogger logger, long messageId);

        [LoggerMessage(Level = LogLevel.Error, Message = "Failed to delete message with id {Id}")]
        public static partial void LogDeleteError(ILogger logger, Exception ex, object id);

        [LoggerMessage(Level = LogLevel.Information, Message = "Adding a range of messages")]
        public static partial void LogAddingRange(ILogger logger);

        [LoggerMessage(Level = LogLevel.Information, Message = "Successfully added a range of messages")]
        public static partial void LogAddedRange(ILogger logger);

        [LoggerMessage(Level = LogLevel.Error, Message = "Failed to add a range of messages")]
        public static partial void LogAddRangeError(ILogger logger, Exception ex);
    }

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

        /// <summary>
        /// Gets all Discord messages asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of Discord messages.</returns>
        public async Task<IEnumerable<DiscordMessage>> GetAllAsync()
        {
            var messages = await context.DiscordMessages.ToListAsync().ConfigureAwait(false);
            return messages;
        }

        /// <summary>
        /// Gets a Discord message by its identifier asynchronously.
        /// </summary>
        /// <param name="id">The identifier of the message.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the Discord message if found; otherwise, null.</returns>
        public async Task<DiscordMessage?> GetByIdAsync(object id)
        {
            var message = await context.DiscordMessages.FindAsync(id).ConfigureAwait(false);
            return message;
        }

        /// <summary>
        /// Adds a new Discord message asynchronously.
        /// </summary>
        /// <param name="entity">The Discord message to add.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the added Discord message.</returns>
        public async Task<DiscordMessage> AddAsync(DiscordMessage entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            using var activity = System.Diagnostics.Activity.Current?.Source.StartActivity("AddMessage");
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

        /// <summary>
        /// Updates an existing Discord message asynchronously.
        /// </summary>
        /// <param name="entity">The Discord message to update.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
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

        /// <summary>
        /// Deletes a Discord message by its identifier asynchronously.
        /// </summary>
        /// <param name="id">The identifier of the message to delete.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
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

        /// <summary>
        /// Gets Discord messages by channel identifier asynchronously.
        /// </summary>
        /// <param name="channelId">The channel identifier.</param>
        /// <param name="skip">The number of messages to skip.</param>
        /// <param name="take">The number of messages to take.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of Discord messages.</returns>
        public async Task<IEnumerable<DiscordMessage>> GetByChannelIdAsync(long channelId, int skip = 0, int take = 100)
        {
            var messages = await context.DiscordMessages.ConfigureAwait(false)
                .Where(m => m.ChannelId == channelId)
                .OrderByDescending(m => m.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync().ConfigureAwait(false);
            return messages;
        }

        /// <summary>
        /// Gets Discord messages by guild identifier asynchronously.
        /// </summary>
        /// <param name="guildId">The guild identifier.</param>
        /// <param name="skip">The number of messages to skip.</param>
        /// <param name="take">The number of messages to take.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of Discord messages.</returns>
        public async Task<IEnumerable<DiscordMessage>> GetByGuildIdAsync(long guildId, int skip = 0, int take = 100)
        {
            var messages = await context.DiscordMessages
                .Where(m => m.GuildId == guildId)
                .OrderByDescending(m => m.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync().ConfigureAwait(false);
            return messages;
        }

        /// <summary>
        /// Gets Discord messages by author identifier asynchronously.
        /// </summary>
        /// <param name="authorId">The author identifier.</param>
        /// <param name="skip">The number of messages to skip.</param>
        /// <param name="take">The number of messages to take.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of Discord messages.</returns>
        public async Task<IEnumerable<DiscordMessage>> GetByAuthorIdAsync(long authorId, int skip = 0, int take = 100)
        {
            var messages = await context.DiscordMessages
                .Where(m => m.AuthorId == authorId)
                .OrderByDescending(m => m.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync().ConfigureAwait(false);
            return messages;
        }

        /// <summary>
        /// Gets thread messages by thread identifier asynchronously.
        /// </summary>
        /// <param name="threadId">The thread identifier.</param>
        /// <param name="skip">The number of messages to skip.</param>
        /// <param name="take">The number of messages to take.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of Discord messages.</returns>
        public async Task<IEnumerable<DiscordMessage>> GetThreadMessagesAsync(long threadId, int skip = 0, int take = 100)
        {
            var messages = await context.DiscordMessages
                .Where(m => m.ThreadId == threadId)
                .OrderByDescending(m => m.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync().ConfigureAwait(false);
            return messages;
        }

        /// <summary>
        /// Adds a range of Discord messages asynchronously.
        /// </summary>
        /// <param name="messages">The collection of Discord messages to add.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the number of state entries written to the database.</returns>
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

        /// <summary>
        /// Updates a range of Discord messages asynchronously.
        /// </summary>
        /// <param name="messages">The collection of Discord messages to update.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the number of state entries written to the database.</returns>
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

        /// <summary>
        /// Deletes a range of Discord messages by their identifiers asynchronously.
        /// </summary>
        /// <param name="messageIds">The collection of message identifiers to delete.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the number of state entries written to the database.</returns>
        public async Task<int> DeleteRangeAsync(IEnumerable<long> messageIds)
        {
            ArgumentNullException.ThrowIfNull(messageIds);

            await using var transaction = await context.Database.BeginTransactionAsync().ConfigureAwait(false);
            try
            {
                var messages = await context.DiscordMessages.ConfigureAwait(false)
                    .Where(m => messageIds.Contains(m.MessageId))
                    .ToListAsync().ConfigureAwait(false);

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
