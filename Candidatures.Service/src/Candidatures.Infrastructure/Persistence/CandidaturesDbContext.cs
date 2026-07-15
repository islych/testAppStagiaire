using Candidatures.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Candidatures.Infrastructure.Persistence;

/// <summary>
/// DbContext pour les candidatures
/// </summary>
public class CandidaturesDbContext : DbContext
{
    /// <summary>
    /// Table des candidatures
    /// </summary>
    public DbSet<Candidature> Candidatures { get; set; }

    public CandidaturesDbContext(DbContextOptions<CandidaturesDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuration de l'entité Candidature
        modelBuilder.ApplyConfiguration(new CandidatureConfiguration());
    }
}
