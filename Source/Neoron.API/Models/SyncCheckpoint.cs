using System.ComponentModel.DataAnnotations;

namespace Neoron.API.Models
{
    /// <summary>
    /// Represents a synchronization checkpoint for Discord entities.
    /// </summary>
    public class SyncCheckpoint
    {
        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        [Key]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the guild ID.
        /// </summary>
        public long GuildId { get; set; }

        /// <summary>
        /// Gets or sets the channel ID.
        /// </summary>
        public long ChannelId { get; set; }

        /// <summary>
        /// Gets or sets the last synced message ID.
        /// </summary>
        public long LastMessageId { get; set; }

        /// <summary>
        /// Gets or sets when this checkpoint was last updated.
        /// </summary>
        public DateTimeOffset LastSyncedAt { get; set; }
    }
}
