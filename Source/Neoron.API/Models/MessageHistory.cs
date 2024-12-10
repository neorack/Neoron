using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Neoron.API.Models
{
    /// <summary>
    /// Represents historical versions of Discord messages.
    /// </summary>
    public class MessageHistory
    {
        /// <summary>
        /// Gets or sets the unique identifier of the history record.
        /// </summary>
        [Key]
        public long HistoryId { get; set; }

        /// <summary>
        /// Gets or sets the message ID this history belongs to.
        /// </summary>
        public long MessageId { get; set; }

        /// <summary>
        /// Gets or sets the previous content.
        /// </summary>
        [Required]
        public required string PreviousContent { get; set; }

        /// <summary>
        /// Gets or sets the previous embedded content.
        /// </summary>
        public string? PreviousEmbeddedContent { get; set; }

        /// <summary>
        /// Gets or sets when this version was created.
        /// </summary>
        public DateTimeOffset ChangedAt { get; set; }

        /// <summary>
        /// Gets or sets the type of change.
        /// </summary>
        public MessageChangeType ChangeType { get; set; }

        /// <summary>
        /// Navigation property to the message.
        /// </summary>
        [ForeignKey(nameof(MessageId))]
        public virtual DiscordMessage? Message { get; set; }
    }

    public enum MessageChangeType
    {
        Created = 0,
        Edited = 1,
        Deleted = 2
    }
}
