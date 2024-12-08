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

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
