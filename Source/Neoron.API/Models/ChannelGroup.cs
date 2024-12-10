using System.ComponentModel.DataAnnotations;

namespace Neoron.API.Models
{
    /// <summary>
    /// Represents a group of channels and their participants.
    /// </summary>
    public class ChannelGroup
    {
        /// <summary>
        /// Gets or sets the unique identifier of the group.
        /// </summary>
        [Key]
        public long GroupId { get; set; }

        /// <summary>
        /// Gets or sets the guild ID this group belongs to.
        /// </summary>
        public long GuildId { get; set; }

        /// <summary>
        /// Gets or sets the name of the group.
        /// </summary>
        [Required]
        [StringLength(100)]
        public required string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the group.
        /// </summary>
        [StringLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets when this group was created.
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets when this group was last active.
        /// </summary>
        public DateTimeOffset LastActiveAt { get; set; }

        /// <summary>
        /// Gets or sets the collection of channel IDs in this group.
        /// </summary>
        public required ICollection<long> ChannelIds { get; set; }

        /// <summary>
        /// Gets or sets the collection of participant user IDs.
        /// </summary>
        public required ICollection<long> ParticipantIds { get; set; }

        /// <summary>
        /// Gets or sets the messages in this group.
        /// </summary>
        public virtual ICollection<DiscordMessage>? Messages { get; set; }
    }
}
