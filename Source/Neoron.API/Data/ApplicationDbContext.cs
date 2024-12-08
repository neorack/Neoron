using Microsoft.EntityFrameworkCore;
using Neoron.API.Models;

namespace Neoron.API.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

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
}
