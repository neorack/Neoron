using Neoron.API.Models;

namespace Neoron.API.DTOs
{
    /// <summary>
    /// Represents a response for a Discord message.
    /// </summary>
    public class MessageResponse
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
        public string Content { get; set; } = null!;

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
        public DateTimeOffset CreatedAt { get; set; }

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
        /// Converts a DiscordMessage entity to a MessageResponse.
        /// </summary>
        /// <param name="entity">The DiscordMessage entity.</param>
        /// <returns>A new MessageResponse.</returns>
        public static MessageResponse FromEntity(DiscordMessage entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            return new MessageResponse
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
                DeletedAt = entity.DeletedAt,
                ReplyToMessageId = entity.ReplyToMessageId,
                ThreadId = entity.ThreadId,
                IsDeleted = entity.IsDeleted,
            };
        }
    }
}
