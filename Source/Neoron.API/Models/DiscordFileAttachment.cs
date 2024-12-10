using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Neoron.API.Models
{
    /// <summary>
    /// Represents a Discord message file attachment.
    /// </summary>
    public class DiscordFileAttachment
    {
        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        [Key]
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the parent message identifier.
        /// </summary>
        public long MessageId { get; set; }

        /// <summary>
        /// Gets or sets the original filename.
        /// </summary>
        [Required]
        [StringLength(260)]
        public required string FileName { get; set; }

        /// <summary>
        /// Gets or sets the content type of the file.
        /// </summary>
        [StringLength(100)]
        public string? ContentType { get; set; }

        /// <summary>
        /// Gets or sets the size of the file in bytes.
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// Gets or sets the FileTable stream ID once processed.
        /// </summary>
        public Guid? FileTableStreamId { get; set; }

        /// <summary>
        /// Gets or sets the staging status of the attachment.
        /// </summary>
        public AttachmentStatus Status { get; set; }

        /// <summary>
        /// Gets or sets when the attachment was created.
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets when the attachment was last processed.
        /// </summary>
        public DateTimeOffset? ProcessedAt { get; set; }

        /// <summary>
        /// Gets or sets any error message from processing.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Navigation property to the associated message.
        /// </summary>
        [ForeignKey(nameof(MessageId))]
        public virtual DiscordMessage? Message { get; set; }
    }

    public enum AttachmentStatus
    {
        Pending = 0,
        Processing = 1,
        Completed = 2,
        Failed = 3
    }
}
