using Candidatures.Domain.Entities;
using Candidatures.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Candidatures.Infrastructure.Persistence;

/// <summary>
/// Configuration EF Core pour l'entité Candidature
/// </summary>
public class CandidatureConfiguration : IEntityTypeConfiguration<Candidature>
{
    public void Configure(EntityTypeBuilder<Candidature> builder)
    {
        // Clé primaire
        builder.HasKey(c => c.Id);

        // Propriétés
        builder.Property(c => c.Id)
            .HasColumnName("Id")
            .ValueGeneratedNever();

        builder.Property(c => c.StagiaireId)
            .HasColumnName("StagiaireId")
            .IsRequired();

        builder.Property(c => c.DepartementId)
            .HasColumnName("DepartementId")
            .IsRequired();

        builder.Property(c => c.SpecialiteId)
            .HasColumnName("SpecialiteId")
            .IsRequired();

        builder.Property(c => c.CvFileName)
            .HasColumnName("CvFileName")
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(c => c.CvPath)
            .HasColumnName("CvPath")
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(c => c.Statut)
            .HasColumnName("Statut")
            .IsRequired()
            .HasDefaultValue(CandidatureStatus.EnAttente)
            .HasConversion(
                v => v.ToString(),
                v => Enum.Parse<CandidatureStatus>(v));

        builder.Property(c => c.EncadrantId)
            .HasColumnName("EncadrantId");

        builder.Property(c => c.Commentaire)
            .HasColumnName("Commentaire")
            .HasMaxLength(2000);

        builder.Property(c => c.DateCreation)
            .HasColumnName("DateCreation")
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(c => c.DateMiseAJour)
            .HasColumnName("DateMiseAJour");

        builder.Property(c => c.DateDecision)
            .HasColumnName("DateDecision");

        builder.Property(c => c.TransmisADirection)
            .HasColumnName("TransmisADirection")
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(c => c.DateTransmissionDirection)
            .HasColumnName("DateTransmissionDirection");

        // Index pour les requêtes fréquentes
        builder.HasIndex(c => c.StagiaireId)
            .HasDatabaseName("IX_Candidature_StagiaireId");

        builder.HasIndex(c => c.EncadrantId)
            .HasDatabaseName("IX_Candidature_EncadrantId");

        builder.HasIndex(c => c.Statut)
            .HasDatabaseName("IX_Candidature_Statut");

        builder.HasIndex(c => new { c.StagiaireId, c.Statut })
            .HasDatabaseName("IX_Candidature_StagiaireId_Statut");

        // Nom de la table
        builder.ToTable("Candidatures", "dbo");
    }
}
