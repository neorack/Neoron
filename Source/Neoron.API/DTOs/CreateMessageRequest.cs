using System.ComponentModel.DataAnnotations;
using Neoron.API.Models;

namespace Neoron.API.DTOs
{
    public record CreateMessageRequest
    {
        [Required]
        public required long MessageId { get; init; }

        [Required]
        public required long ChannelId { get; init; }

        [Required]
        public required long GuildId { get; init; }

        [Required]
        public required long AuthorId { get; init; }

        [Required]
        [StringLength(2000)]
        public required string Content { get; init; }

        public string? EmbeddedContent { get; init; }

        public MessageType MessageType { get; init; }

        public long? ReplyToMessageId { get; init; }

        public long? ThreadId { get; init; }

        public DiscordMessage ToEntity() => new()
        {
            MessageId = MessageId,
            ChannelId = ChannelId,
            GuildId = GuildId,
            AuthorId = AuthorId,
            Content = Content,
            EmbeddedContent = EmbeddedContent,
            MessageType = MessageType,
            CreatedAt = DateTimeOffset.UtcNow,
            ReplyToMessageId = ReplyToMessageId,
            ThreadId = ThreadId
        };
    }
}
