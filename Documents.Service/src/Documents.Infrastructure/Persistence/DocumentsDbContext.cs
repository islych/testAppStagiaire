using Documents.Domain.Entities;
using Documents.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Documents.Infrastructure.Persistence;

/// <summary>
/// DbContext pour le service Documents
/// </summary>
public class DocumentsDbContext : DbContext
{
    /// <summary>
    /// Table des documents administratifs
    /// </summary>
    public DbSet<Document> Documents { get; set; }

    public DocumentsDbContext(DbContextOptions<DocumentsDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new DocumentConfiguration());
    }
}
