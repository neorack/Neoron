/*
 * Entity representing a participant's membership in a channel group
 * Manages group participation and role assignments
 * 
 * Key design decisions:
 * - Tracks participation timestamps
 * - Supports role-based permissions
 * - Monitors member activity
 * - Uses composite key pattern
 * - Implements group navigation
 */

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Neoron.API.Models
{
    /// <summary>
    /// Represents a participant's membership and role in a channel group.
    /// </summary>
    /// <remarks>
    /// This entity:
    /// - Tracks when members join groups
    /// - Records member activity timestamps
    /// - Manages member roles and permissions
    /// - Supports group relationship navigation
    /// - Uses composite key of GroupId and UserId
    /// </remarks>
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
        [Required]
        public DateTimeOffset JoinedAt { get; set; }

        /// <summary>
        /// Gets or sets when this participant was last active.
        /// </summary>
        public DateTimeOffset? LastActiveAt { get; set; }

        /// <summary>
        /// Gets or sets the participant's role in the group.
        /// </summary>
        [Required]
        public GroupRole Role { get; set; } = GroupRole.Member;

        /// <summary>
        /// Navigation property to the associated group.
        /// </summary>
        [ForeignKey(nameof(GroupId))]
        public virtual ChannelGroup? Group { get; set; }
    }
}
    public enum GroupRole
    {
        Member = 0,
        Moderator = 1,
        Admin = 2
    }
