using System.ComponentModel.DataAnnotations;

namespace Neoron.API.Models
{
    /// <summary>
    /// Represents a staged Discord message pending processing.
    /// </summary>
    public class StagedDiscordMessage
    {
        /// <summary>
        /// Gets or sets the staging record ID.
        /// </summary>
        [Key]
        public long StagingId { get; set; }

        /// <summary>
        /// Gets or sets the Discord message data.
        /// </summary>
        public required DiscordMessage Message { get; set; }

        /// <summary>
        /// Gets or sets the staging status.
        /// </summary>
        public StagingStatus Status { get; set; }

        /// <summary>
        /// Gets or sets when the message was staged.
        /// </summary>
        public DateTimeOffset StagedAt { get; set; }

        /// <summary>
        /// Gets or sets when the message was processed.
        /// </summary>
        public DateTimeOffset? ProcessedAt { get; set; }

        /// <summary>
        /// Gets or sets any error message from processing.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the retry count.
        /// </summary>
        public int RetryCount { get; set; }
    }

    public enum StagingStatus
    {
        Pending = 0,
        Processing = 1,
        Completed = 2,
        Failed = 3,
        Duplicate = 4
    }
}
