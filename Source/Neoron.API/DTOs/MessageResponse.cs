using Neoron.API.Models;

namespace Neoron.API.DTOs;

public record MessageResponse
{
    public required long MessageId { get; init; }
    public required long ChannelId { get; init; }
    public required long GuildId { get; init; }
    public required long AuthorId { get; init; }
    public required string Content { get; init; }
    public string? EmbeddedContent { get; init; }
    public MessageType MessageType { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? EditedAt { get; init; }
    public long? ReplyToMessageId { get; init; }
    public long? ThreadId { get; init; }

    public static MessageResponse FromEntity(DiscordMessage entity) => new()
    {
        MessageId = entity.MessageId,
        ChannelId = entity.ChannelId,
        GuildId = entity.GuildId,
        AuthorId = entity.AuthorId,
        Content = entity.Content,
        EmbeddedContent = entity.EmbeddedContent,
        MessageType = entity.MessageType,
        CreatedAt = entity.CreatedAt,
        EditedAt = entity.EditedAt,
        ReplyToMessageId = entity.ReplyToMessageId,
        ThreadId = entity.ThreadId
    };
}
