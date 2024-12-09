using NUnit.Framework;
using System;

[TestFixture]
public class DiscordMessageBuilderTests
{
    [Test]
    public void Create_ShouldInitializeWithDefaultValues()
    {
        // Arrange
        var builder = new DiscordMessageBuilder();

        // Act
        var message = builder.Create();

        // Assert
        Assert.IsNotNull(message);
        Assert.IsNull(message.Content);
        Assert.IsNull(message.EmbeddedContent);
        Assert.AreEqual(default(long), message.MessageId);
        Assert.AreEqual(default(long), message.ChannelId);
        Assert.AreEqual(default(long), message.GuildId);
        Assert.AreEqual(default(long), message.AuthorId);
        Assert.AreEqual(MessageType.Regular, message.MessageType);
        Assert.AreEqual(default(DateTimeOffset), message.CreatedAt);
        Assert.AreEqual(default(DateTimeOffset?), message.EditedAt);
        Assert.AreEqual(default(DateTimeOffset?), message.DeletedAt);
        Assert.IsNull(message.ReplyToMessageId);
        Assert.IsNull(message.ThreadId);
        Assert.IsFalse(message.IsDeleted);
        Assert.IsNull(message.ReplyToMessage);
        Assert.IsNull(message.ThreadParent);
        Assert.IsNull(message.Replies);
        Assert.IsNull(message.ThreadMessages);
    }

    [Test]
    public void SetProperties_ShouldApplyCorrectly()
    {
        // Arrange
        var builder = new DiscordMessageBuilder();
        var expectedContent = "Hello, World!";
        var expectedEmbeddedContent = "Embedded Content";
        var expectedAuthorId = Guid.NewGuid();
        var expectedChannelId = 12345L;
        var expectedGuildId = 67890L;
        var expectedMessageType = MessageType.System;

        // Act
        builder.SetContent(expectedContent);
        builder.SetEmbeddedContent(expectedEmbeddedContent);
        builder.SetAuthorId(expectedAuthorId);
        builder.SetChannelId(expectedChannelId);
        builder.SetGuildId(expectedGuildId);
        builder.SetMessageType(expectedMessageType);
        var message = builder.Create();

        // Assert
        Assert.AreEqual(expectedContent, message.Content);
        Assert.AreEqual(expectedEmbeddedContent, message.EmbeddedContent);
        Assert.AreEqual(expectedAuthorId, message.AuthorId);
        Assert.AreEqual(expectedChannelId, message.ChannelId);
        Assert.AreEqual(expectedGuildId, message.GuildId);
        Assert.AreEqual(expectedMessageType, message.MessageType);
        // Add more assertions for other properties if needed
    }
}
