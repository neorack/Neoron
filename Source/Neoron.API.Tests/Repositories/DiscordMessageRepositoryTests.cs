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

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
