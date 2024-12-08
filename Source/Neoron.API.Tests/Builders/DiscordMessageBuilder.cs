using Neoron.API.Models;

namespace Neoron.API.Tests.Builders;

public class DiscordMessageBuilder
{
    private long _messageId = 123456789;
    private long _channelId = 987654321;
    private long _guildId = 11111111;
    private long _authorId = 22222222;
    private string _content = "Test message";
    private int _messageType = 0;
    private DateTimeOffset _createdAt = DateTimeOffset.UtcNow;

    public DiscordMessageBuilder WithMessageId(long messageId)
    {
        _messageId = messageId;
        return this;
    }

    public DiscordMessageBuilder WithChannelId(long channelId)
    {
        _channelId = channelId;
        return this;
    }

    public DiscordMessageBuilder WithContent(string content)
    {
        _content = content;
        return this;
    }

    public DiscordMessageBuilder WithCreatedAt(DateTimeOffset createdAt)
    {
        _createdAt = createdAt;
        return this;
    }

    public DiscordMessageBuilder WithAuthorId(long authorId)
    {
        _authorId = authorId;
        return this;
    }

    public DiscordMessageBuilder WithGuildId(long guildId)
    {
        _guildId = guildId;
        return this;
    }

    public DiscordMessageBuilder WithMessageType(int messageType)
    {
        _messageType = messageType;
        return this;
    }

    private string? _embeddedContent;

    public DiscordMessageBuilder WithEmbeddedContent(string embeddedContent)
    {
        _embeddedContent = embeddedContent;
        return this;
    }

    public DiscordMessage Build()
    {
        return new DiscordMessage
        {
            MessageId = _messageId,
            ChannelId = _channelId,
            GuildId = _guildId,
            AuthorId = _authorId,
            Content = _content,
            EmbeddedContent = _embeddedContent,
            MessageType = _messageType,
            CreatedAt = _createdAt
        };
    }

    public static DiscordMessageBuilder Create()
    {
        return new DiscordMessageBuilder();
    }
}
