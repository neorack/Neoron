namespace Neoron.API.Models;

public class DiscordMessage
{
    public long MessageId { get; set; }
    public long ChannelId { get; set; }
    public long GuildId { get; set; }
    public long AuthorId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? EmbeddedContent { get; set; }
    public MessageType MessageType { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? EditedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public long? ReplyToMessageId { get; set; }
    public long? ThreadId { get; set; }
    public bool IsDeleted { get; set; }

    public virtual DiscordMessage? ReplyToMessage { get; set; }
    public virtual DiscordMessage? ThreadParent { get; set; }
    public virtual ICollection<DiscordMessage>? Replies { get; set; }
    public virtual ICollection<DiscordMessage>? ThreadMessages { get; set; }
}

public enum MessageType : byte
{
    Regular = 0,
    System = 1
}
