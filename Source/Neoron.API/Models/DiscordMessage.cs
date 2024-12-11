/*
 * Core entity representing a Discord message in the system
 * Handles message content, relationships, and tracking metadata
 * 
 * Key design decisions:
 * - Uses soft delete pattern with IsDeleted flag
 * - Tracks message history for audit purposes
 * - Supports threaded conversations and replies
 * - Uses optimistic concurrency with Version property
 * - Implements content validation
 * - Supports embedded content
 * - Maintains audit trail
 * - Handles group organization
 * - Tracks sync status
 */

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Neoron.API.Models
{
    /// <summary>
    /// Represents a Discord message entity with full tracking and relationship support.
    /// </summary>
    /// <remarks>
    /// This entity maintains the complete lifecycle of a Discord message including:
    /// - Creation and modification timestamps
    /// - Soft deletion support
    /// - Thread/reply relationships
    /// - Channel and group organization
    /// - Version tracking for concurrency
    /// - History tracking for auditing
    /// </remarks>
    public class DiscordMessage
    {
        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        public long Id { get; set; }

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
        [ForeignKey(nameof(ReplyToMessageId))]
        public virtual DiscordMessage? ReplyToMessage { get; set; }

        /// <summary>
        /// Gets or sets the parent thread message.
        /// </summary>
        [ForeignKey(nameof(ThreadId))]
        public virtual DiscordMessage? ThreadParent { get; set; }

        /// <summary>
        /// Gets or sets the collection of replies to this message.
        /// </summary>
        [InverseProperty(nameof(ReplyToMessage))]
        public virtual ICollection<DiscordMessage>? Replies { get; set; }

        /// <summary>
        /// Gets or sets the collection of messages in this thread.
        /// </summary>
        [InverseProperty(nameof(ThreadParent))]
        public virtual ICollection<DiscordMessage>? ThreadMessages { get; set; }

        /// <summary>
        /// Gets or sets the version number for optimistic concurrency.
        /// </summary>
        [Timestamp]
        public byte[] Version { get; set; } = Array.Empty<byte>();

        /// <summary>
        /// Gets or sets when this message was last synced.
        /// </summary>
        public DateTimeOffset LastSyncedAt { get; set; }

        /// <summary>
        /// Gets or sets the previous versions of this message.
        /// </summary>
        public virtual ICollection<MessageHistory>? History { get; set; }

        /// <summary>
        /// Gets or sets the channel group this message belongs to.
        /// </summary>
        public long? GroupId { get; set; }

        /// <summary>
        /// Navigation property to the associated channel group.
        /// </summary>
        [ForeignKey(nameof(GroupId))]
        public virtual ChannelGroup? Group { get; set; }
    }
}
