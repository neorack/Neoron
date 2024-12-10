using System.ComponentModel.DataAnnotations;
using Neoron.API.Models;

namespace Neoron.API.DTOs
{
    /// <summary>
    /// Represents a request to update an existing Discord message.
    /// </summary>
    public class UpdateMessageRequest
    {
        /// <summary>
        /// Gets or sets the content of the message.
        /// </summary>
        [StringLength(2000, MinimumLength = 1)]
        public string? Content { get; set; }

        /// <summary>
        /// Gets or sets optional embedded content of the message.
        /// </summary>
        public string? EmbeddedContent { get; set; }

        /// <summary>
        /// Gets or sets the type of message.
        /// </summary>
        public MessageType? MessageType { get; set; }

        /// <summary>
        /// Updates the properties of a DiscordMessage entity with the values from this request.
        /// </summary>
        /// <param name="entity">The DiscordMessage entity to update.</param>
        public void UpdateEntity(DiscordMessage entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            if (Content != null)
            {
                entity.Content = Content;
            }

            if (EmbeddedContent != null)
            {
                entity.EmbeddedContent = EmbeddedContent;
            }

            if (MessageType.HasValue)
            {
                entity.MessageType = MessageType.Value;
            }
        }
    }
}
