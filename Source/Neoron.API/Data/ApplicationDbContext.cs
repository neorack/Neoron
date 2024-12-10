using System.Threading;
using System.Threading.Tasks;
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

            modelBuilder.Entity<DiscordMessage>(entity =>
            {
                entity.HasKey(e => e.MessageId)
                    .IsClustered();

                entity.HasOne(d => d.ReplyToMessage)
                    .WithMany(p => p.Replies)
                    .HasForeignKey(d => d.ReplyToMessageId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.ThreadParent)
                    .WithMany(p => p.ThreadMessages)
                    .HasForeignKey(d => d.ThreadId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
