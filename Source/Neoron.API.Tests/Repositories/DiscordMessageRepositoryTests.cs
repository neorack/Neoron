using Microsoft.EntityFrameworkCore;
using Neoron.API.Data;
using Neoron.API.Models;
using Neoron.API.Repositories;
using Xunit;

namespace Neoron.API.Tests.Repositories;

public class DiscordMessageRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly DiscordMessageRepository _repository;

    public DiscordMessageRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new DiscordMessageRepository(_context);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllMessages()
    {
        // Arrange
        var messages = new List<DiscordMessage>
        {
            new() { MessageId = 1, Content = "Test 1", CreatedAt = DateTimeOffset.UtcNow },
            new() { MessageId = 2, Content = "Test 2", CreatedAt = DateTimeOffset.UtcNow }
        };
        await _context.DiscordMessages.AddRangeAsync(messages);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsCorrectMessage()
    {
        // Arrange
        var message = new DiscordMessage 
        { 
            MessageId = 1, 
            Content = "Test", 
            CreatedAt = DateTimeOffset.UtcNow 
        };
        await _context.DiscordMessages.AddAsync(message);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(1L);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test", result.Content);
    }

    [Fact]
    public async Task AddAsync_AddsNewMessage()
    {
        // Arrange
        var message = new DiscordMessage 
        { 
            MessageId = 1, 
            Content = "Test", 
            CreatedAt = DateTimeOffset.UtcNow 
        };

        // Act
        await _repository.AddAsync(message);

        // Assert
        var result = await _context.DiscordMessages.FindAsync(1L);
        Assert.NotNull(result);
        Assert.Equal("Test", result.Content);
    }

    [Fact]
    public async Task DeleteAsync_SoftDeletesMessage()
    {
        // Arrange
        var message = new DiscordMessage 
        { 
            MessageId = 1, 
            Content = "Test", 
            CreatedAt = DateTimeOffset.UtcNow 
        };
        await _context.DiscordMessages.AddAsync(message);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(1L);

        // Assert
        var result = await _context.DiscordMessages.FindAsync(1L);
        Assert.NotNull(result);
        Assert.True(result.IsDeleted);
        Assert.NotNull(result.DeletedAt);
    }

    [Fact]
    public async Task GetByChannelIdAsync_ReturnsCorrectMessages()
    {
        // Arrange
        var messages = new List<DiscordMessage>
        {
            new() { MessageId = 1, ChannelId = 100, Content = "Test 1", CreatedAt = DateTimeOffset.UtcNow },
            new() { MessageId = 2, ChannelId = 100, Content = "Test 2", CreatedAt = DateTimeOffset.UtcNow },
            new() { MessageId = 3, ChannelId = 200, Content = "Test 3", CreatedAt = DateTimeOffset.UtcNow }
        };
        await _context.DiscordMessages.AddRangeAsync(messages);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByChannelIdAsync(100);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, message => Assert.Equal(100, message.ChannelId));
    }

    [Fact]
    public async Task GetByGuildIdAsync_ReturnsCorrectMessages()
    {
        // Arrange
        var messages = new List<DiscordMessage>
        {
            new() { MessageId = 1, GuildId = 500, Content = "Test 1", CreatedAt = DateTimeOffset.UtcNow },
            new() { MessageId = 2, GuildId = 500, Content = "Test 2", CreatedAt = DateTimeOffset.UtcNow },
            new() { MessageId = 3, GuildId = 600, Content = "Test 3", CreatedAt = DateTimeOffset.UtcNow }
        };
        await _context.DiscordMessages.AddRangeAsync(messages);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByGuildIdAsync(500);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, message => Assert.Equal(500, message.GuildId));
    }

    [Fact]
    public async Task GetByAuthorIdAsync_ReturnsCorrectMessages()
    {
        // Arrange
        var messages = new List<DiscordMessage>
        {
            new() { MessageId = 1, AuthorId = 777, Content = "Test 1", CreatedAt = DateTimeOffset.UtcNow },
            new() { MessageId = 2, AuthorId = 777, Content = "Test 2", CreatedAt = DateTimeOffset.UtcNow },
            new() { MessageId = 3, AuthorId = 888, Content = "Test 3", CreatedAt = DateTimeOffset.UtcNow }
        };
        await _context.DiscordMessages.AddRangeAsync(messages);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByAuthorIdAsync(777);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, message => Assert.Equal(777, message.AuthorId));
    }

    [Fact]
    public async Task GetThreadMessagesAsync_ReturnsCorrectMessages()
    {
        // Arrange
        var messages = new List<DiscordMessage>
        {
            new() { MessageId = 1, ThreadId = 999, Content = "Thread 1", CreatedAt = DateTimeOffset.UtcNow },
            new() { MessageId = 2, ThreadId = 999, Content = "Thread 2", CreatedAt = DateTimeOffset.UtcNow },
            new() { MessageId = 3, ThreadId = 888, Content = "Other Thread", CreatedAt = DateTimeOffset.UtcNow }
        };
        await _context.DiscordMessages.AddRangeAsync(messages);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetThreadMessagesAsync(999);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.All(result, message => Assert.Equal(999, message.ThreadId));
    }

    [Fact]
    public async Task UpdateAsync_UpdatesExistingMessage()
    {
        // Arrange
        var message = new DiscordMessage 
        { 
            MessageId = 1, 
            Content = "Original", 
            CreatedAt = DateTimeOffset.UtcNow 
        };
        await _context.DiscordMessages.AddAsync(message);
        await _context.SaveChangesAsync();

        // Act
        message.Content = "Updated";
        await _repository.UpdateAsync(message);

        // Assert
        var updated = await _context.DiscordMessages.FindAsync(1L);
        Assert.NotNull(updated);
        Assert.Equal("Updated", updated.Content);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonexistentId_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(999L);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_WithNonexistentId_DoesNotThrowException()
    {
        // Act & Assert
        await _repository.DeleteAsync(999L);
        // Should complete without throwing
    }

    [Fact]
    public async Task GetByChannelIdAsync_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        var messages = Enumerable.Range(1, 20).Select(i => new DiscordMessage
        {
            MessageId = i,
            ChannelId = 100,
            Content = $"Test {i}",
            CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-i)
        }).ToList();
        await _context.DiscordMessages.AddRangeAsync(messages);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByChannelIdAsync(100, skip: 5, take: 5);

        // Assert
        Assert.Equal(5, result.Count());
        Assert.Equal("Test 6", result.First().Content);
    }

    [Fact]
    public async Task AddRangeAsync_AddsMultipleMessages()
    {
        // Arrange
        var messages = Enumerable.Range(1, 5).Select(i => new DiscordMessage
        {
            MessageId = i,
            Content = $"Bulk Test {i}",
            CreatedAt = DateTimeOffset.UtcNow
        }).ToList();

        // Act
        var result = await _repository.AddRangeAsync(messages);

        // Assert
        Assert.Equal(5, result);
        Assert.Equal(5, await _context.DiscordMessages.CountAsync());
    }

    [Fact]
    public async Task UpdateRangeAsync_UpdatesMultipleMessages()
    {
        // Arrange
        var messages = Enumerable.Range(1, 3).Select(i => new DiscordMessage
        {
            MessageId = i,
            Content = $"Original {i}",
            CreatedAt = DateTimeOffset.UtcNow
        }).ToList();
        await _context.DiscordMessages.AddRangeAsync(messages);
        await _context.SaveChangesAsync();

        foreach (var msg in messages)
        {
            msg.Content = $"Updated {msg.MessageId}";
        }

        // Act
        var result = await _repository.UpdateRangeAsync(messages);

        // Assert
        Assert.Equal(3, result);
        var updated = await _context.DiscordMessages.ToListAsync();
        Assert.All(updated, msg => Assert.StartsWith("Updated", msg.Content));
    }

    [Fact]
    public async Task DeleteRangeAsync_SoftDeletesMultipleMessages()
    {
        // Arrange
        var messages = Enumerable.Range(1, 4).Select(i => new DiscordMessage
        {
            MessageId = i,
            Content = $"Test {i}",
            CreatedAt = DateTimeOffset.UtcNow
        }).ToList();
        await _context.DiscordMessages.AddRangeAsync(messages);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteRangeAsync(new[] { 1L, 3L });

        // Assert
        Assert.Equal(2, result);
        var deletedMessages = await _context.DiscordMessages
            .Where(m => m.MessageId == 1 || m.MessageId == 3)
            .ToListAsync();
        Assert.All(deletedMessages, msg =>
        {
            Assert.True(msg.IsDeleted);
            Assert.NotNull(msg.DeletedAt);
        });
    }

    [Fact]
    public async Task ConcurrentUpdates_HandlesCorrectly()
    {
        // Arrange
        var message = new DiscordMessage 
        { 
            MessageId = 1, 
            Content = "Original", 
            CreatedAt = DateTimeOffset.UtcNow 
        };
        await _context.DiscordMessages.AddAsync(message);
        await _context.SaveChangesAsync();

        // Create two repositories simulating concurrent access
        var repo1 = new DiscordMessageRepository(_context);
        var repo2 = new DiscordMessageRepository(_context);

        // Act & Assert
        var message1 = await _context.DiscordMessages.FindAsync(1L);
        var message2 = await _context.DiscordMessages.FindAsync(1L);

        message1!.Content = "Update 1";
        message2!.Content = "Update 2";

        await repo1.UpdateAsync(message1);
        
        // Second update should throw DbUpdateConcurrencyException
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => 
            repo2.UpdateAsync(message2));
    }

    [Fact]
    public async Task ConnectionResilience_HandlesRetry()
    {
        // Arrange
        var message = new DiscordMessage 
        { 
            MessageId = 1, 
            Content = "Test", 
            CreatedAt = DateTimeOffset.UtcNow 
        };

        // Act & Assert
        // Simulate temporary connection issue and retry
        var retryCount = 0;
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            if (retryCount++ == 0)
                throw new DbUpdateException("Simulated connection error", new Exception());

            await _repository.AddAsync(message);
            await transaction.CommitAsync();
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync();
            // Retry once
            await _repository.AddAsync(message);
        }

        var result = await _context.DiscordMessages.FindAsync(1L);
        Assert.NotNull(result);
        Assert.Equal("Test", result.Content);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
