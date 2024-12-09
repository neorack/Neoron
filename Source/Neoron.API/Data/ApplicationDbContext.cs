using Neoron.API.Models;

namespace Neoron.API.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options), DbContext
    {
        public DbSet<DiscordMessage> DiscordMessages { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
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
    }
}
