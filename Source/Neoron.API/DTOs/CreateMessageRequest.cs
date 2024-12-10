using System.ComponentModel.DataAnnotations;
using Neoron.API.Models;

namespace Neoron.API.DTOs
{
    /// <summary>
    /// Represents a request to create a new Discord message.
    /// </summary>
    public class CreateMessageRequest
    {
        /// <summary>
        /// Gets or sets the channel identifier where the message will be sent.
        /// </summary>
        [Required]
        public long ChannelId { get; set; }

        /// <summary>
        /// Gets or sets the guild identifier where the message belongs.
        /// </summary>
        [Required]
        public long GuildId { get; set; }

        /// <summary>
        /// Gets or sets the author identifier of the message.
        /// </summary>
        [Required]
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
        /// Converts the CreateMessageRequest to a DiscordMessage entity.
        /// </summary>
        /// <returns>A new DiscordMessage entity.</returns>
        public DiscordMessage ToEntity()
        {
            return new DiscordMessage
            {
                ChannelId = ChannelId,
                GuildId = GuildId,
                AuthorId = AuthorId,
                Content = Content,
                EmbeddedContent = EmbeddedContent,
                MessageType = MessageType,
                CreatedAt = CreatedAt,
            };
        }
    }
}
