using System.ComponentModel.DataAnnotations;
using Neoron.API.Models;

namespace Neoron.API.DTOs
{
    public record UpdateMessageRequest
    {
        [Required]
        [StringLength(2000)]
        public required string Content { get; init; }

        public string? EmbeddedContent { get; init; }

        public void UpdateEntity(DiscordMessage entity)
        {
            entity.Content = Content;
            entity.EmbeddedContent = EmbeddedContent;
            entity.EditedAt = DateTimeOffset.UtcNow;
        }
    }
}
