using Microsoft.EntityFrameworkCore;
using Neoron.API.Models;

namespace Neoron.API.Data
{
    /// <summary>
    /// Represents the database context for the application.
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationDbContext"/> class.
        /// </summary>
        /// <param name="options">The options to be used by a <see cref="DbContext"/>.</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Gets or sets the Discord messages.
        /// </summary>
        public DbSet<DiscordMessage> DiscordMessages { get; set; } = null!;

        /// <summary>
        /// Gets or sets the channel groups.
        /// </summary>
        public DbSet<ChannelGroup> ChannelGroups { get; set; } = null!;

        /// <summary>
        /// Gets or sets the group channels.
        /// </summary>
        public DbSet<GroupChannel> GroupChannels { get; set; } = null!;

        /// <summary>
        /// Gets or sets the group participants.
        /// </summary>
        public DbSet<GroupParticipant> GroupParticipants { get; set; } = null!;

        /// <summary>
        /// Gets or sets the message history.
        /// </summary>
        public DbSet<MessageHistory> MessageHistory { get; set; } = null!;

        /// <summary>
        /// Gets or sets the staged messages.
        /// </summary>
        public DbSet<StagedDiscordMessage> StagedMessages { get; set; } = null!;

        /// <summary>
        /// Gets or sets the file attachments.
        /// </summary>
        public DbSet<DiscordFileAttachment> FileAttachments { get; set; } = null!;

        /// <summary>
        /// Gets or sets the sync checkpoints.
        /// </summary>
        public DbSet<SyncCheckpoint> SyncCheckpoints { get; set; } = null!;

        /// <summary>
        /// Asynchronously saves all changes made in this context to the database.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous save operation. The task result contains the number of state entries written to the database.</returns>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<DiscordMessage>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = DateTimeOffset.UtcNow;
                        break;
                    case EntityState.Modified:
                        entry.Entity.EditedAt = DateTimeOffset.UtcNow;
                        break;
                    default:
                        break;
                }
            }

            return await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Configures the model for the context.
        /// </summary>
        /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ArgumentNullException.ThrowIfNull(modelBuilder);

            _ = modelBuilder.Entity<DiscordMessage>(entity =>
            {
                _ = entity.HasKey(e => e.Id)
                    .IsClustered();

                _ = entity.HasOne(d => d.ReplyToMessage)
                    .WithMany(p => p.Replies)
                    .HasForeignKey(d => d.ReplyToMessageId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasOne(d => d.ThreadParent)
                    .WithMany(p => p.ThreadMessages)
                    .HasForeignKey(d => d.ThreadId)
                    .OnDelete(DeleteBehavior.Restrict);

                _ = entity.HasOne(d => d.Group)
                    .WithMany(g => g.Messages)
                    .HasForeignKey(d => d.GroupId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            _ = modelBuilder.Entity<GroupChannel>(entity =>
            {
                _ = entity.HasKey(e => new { e.GroupId, e.ChannelId });
            });

            _ = modelBuilder.Entity<GroupParticipant>(entity =>
            {
                _ = entity.HasKey(e => new { e.GroupId, e.UserId });
            });

            _ = modelBuilder.Entity<MessageHistory>(entity =>
            {
                _ = entity.HasOne(h => h.Message)
                    .WithMany(m => m.History)
                    .HasForeignKey(h => h.MessageId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            _ = modelBuilder.Entity<DiscordFileAttachment>(entity =>
            {
                _ = entity.HasOne(a => a.Message)
                    .WithMany()
                    .HasForeignKey(a => a.MessageId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            _ = modelBuilder.Entity<SyncCheckpoint>(entity =>
            {
                _ = entity.HasIndex(e => new { e.GuildId, e.ChannelId })
                    .IsUnique();
            });
        }
    }
}
