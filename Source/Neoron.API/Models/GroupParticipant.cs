using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Neoron.API.Models
{
    /// <summary>
    /// Represents a participant in a channel group.
    /// </summary>
    public class GroupParticipant
    {
        /// <summary>
        /// Gets or sets the group identifier.
        /// </summary>
        public long GroupId { get; set; }

        /// <summary>
        /// Gets or sets the user identifier.
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// Gets or sets when this participant joined the group.
        /// </summary>
        public DateTimeOffset JoinedAt { get; set; }

        /// <summary>
        /// Navigation property to the associated group.
        /// </summary>
        [ForeignKey(nameof(GroupId))]
        public virtual ChannelGroup? Group { get; set; }
    }
}
