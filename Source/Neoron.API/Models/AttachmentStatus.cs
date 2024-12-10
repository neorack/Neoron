using System;

namespace Neoron.API.Models
{
    /// <summary>
    /// Represents the status of a file attachment.
    /// </summary>
    public enum AttachmentStatus
    {
        /// <summary>
        /// The attachment is pending processing.
        /// </summary>
        Pending = 0,

        /// <summary>
        /// The attachment is currently being processed.
        /// </summary>
        Processing = 1,

        /// <summary>
        /// The attachment has been processed successfully.
        /// </summary>
        Completed = 2,

        /// <summary>
        /// The attachment processing has failed.
        /// </summary>
        Failed = 3,
    }
}
