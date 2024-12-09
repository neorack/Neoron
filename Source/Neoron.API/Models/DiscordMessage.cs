namespace Neoron.API.Models;

/// <summary>
/// Represents a Discord message entity.
/// </summary>
public class DiscordMessage
{
    /// <summary>
    /// Gets or sets the unique identifier of the message.
    /// </summary>
    public long MessageId { get; set; }

    /// <summary>
    /// Gets or sets the channel identifier where the message was sent.
    /// </summary>
    public long ChannelId { get; set; }

    /// <summary>
    /// Gets or sets the guild identifier where the message belongs.
    /// </summary>
    public long GuildId { get; set; }

    /// <summary>
    /// Gets or sets the author identifier of the message.
    /// </summary>
    public long AuthorId { get; set; }

    /// <summary>
    /// Gets or sets the content of the message.
    /// </summary>
    [Required]
    [StringLength(2000, MinimumLength = 1)]
    public required string Content { get; set; }

    /// <summary>
    /// Gets or sets optional embedded content of the message.
    /// </summary>
    public string? EmbeddedContent { get; set; }

    /// <summary>
    /// Gets or sets the type of message.
    /// </summary>
    public MessageType MessageType { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp of the message.
    /// </summary>
    public required DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the last edit timestamp of the message.
    /// </summary>
    public DateTimeOffset? EditedAt { get; set; }

    /// <summary>
    /// Gets or sets the deletion timestamp of the message.
    /// </summary>
    public DateTimeOffset? DeletedAt { get; set; }

    /// <summary>
    /// Gets or sets the message ID this message is replying to.
    /// </summary>
    public long? ReplyToMessageId { get; set; }

    /// <summary>
    /// Gets or sets the thread ID this message belongs to.
    /// </summary>
    public long? ThreadId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the message is deleted.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Gets or sets the message being replied to.
    /// </summary>
    public virtual DiscordMessage? ReplyToMessage { get; set; }

    /// <summary>
    /// Gets or sets the parent thread message.
    /// </summary>
    public virtual DiscordMessage? ThreadParent { get; set; }

    /// <summary>
    /// Gets or sets the collection of replies to this message.
    /// </summary>
    public virtual ICollection<DiscordMessage>? Replies { get; set; }

    /// <summary>
    /// Gets or sets the collection of messages in this thread.
    /// </summary>
    public virtual ICollection<DiscordMessage>? ThreadMessages { get; set; }
}

public enum MessageType : byte
{
    Regular = 0,
    System = 1
}
