using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Neoron.API.Models
{
    /// <summary>
    /// Represents a channel belonging to a group.
    /// </summary>
    public class GroupChannel
    {
        /// <summary>
        /// Gets or sets the group identifier.
        /// </summary>
        public long GroupId { get; set; }

        /// <summary>
        /// Gets or sets the channel identifier.
        /// </summary>
        public long ChannelId { get; set; }

        /// <summary>
        /// Gets or sets when this channel was added to the group.
        /// </summary>
        public DateTimeOffset AddedAt { get; set; }

        /// <summary>
        /// Navigation property to the associated group.
        /// </summary>
        [ForeignKey(nameof(GroupId))]
        public virtual ChannelGroup? Group { get; set; }
    }
}
