using Microsoft.EntityFrameworkCore;
using Notifications.Domain.Entities;

namespace Notifications.Infrastructure.Persistence;

public class NotificationsDbContext : DbContext
{
    public NotificationsDbContext(DbContextOptions<NotificationsDbContext> options) : base(options) { }

    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Notification>(e =>
        {
            e.HasKey(n => n.Id);
            e.Property(n => n.Titre).HasMaxLength(300).IsRequired();
            e.Property(n => n.Message).HasMaxLength(2000).IsRequired();
            e.Property(n => n.Type).HasConversion<string>().HasMaxLength(100);
            e.HasIndex(n => n.DestinataireId);
            e.HasIndex(n => new { n.DestinataireId, n.EstLue });
        });
    }
}
