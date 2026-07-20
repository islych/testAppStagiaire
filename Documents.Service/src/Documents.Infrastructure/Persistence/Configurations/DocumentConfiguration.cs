using Documents.Domain.Entities;
using Documents.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Documents.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuration EF Core Fluent API pour l'entité Document
/// </summary>
public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        // Clé primaire
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Id)
            .HasColumnName("Id")
            .ValueGeneratedNever();

        // Stagiaire & candidature
        builder.Property(d => d.StagiaireId)
            .HasColumnName("StagiaireId")
            .IsRequired();

        builder.Property(d => d.CandidatureId)
            .HasColumnName("CandidatureId");

        // Type — stocké en string pour la lisibilité en base
        builder.Property(d => d.Type)
            .HasColumnName("Type")
            .IsRequired()
            .HasMaxLength(50)
            .HasConversion(
                v => v.ToString(),
                v => ParseTypeDocument(v));

        // Fichier
        builder.Property(d => d.NomFichier)
            .HasColumnName("NomFichier")
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(d => d.NomFichierStockage)
            .HasColumnName("NomFichierStockage")
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(d => d.CheminFichier)
            .HasColumnName("CheminFichier")
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(d => d.Extension)
            .HasColumnName("Extension")
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(d => d.TailleFichierOctets)
            .HasColumnName("TailleFichierOctets")
            .IsRequired();

        builder.Property(d => d.ContentType)
            .HasColumnName("ContentType")
            .IsRequired()
            .HasMaxLength(100);

        // Statut — stocké en string
        builder.Property(d => d.Statut)
            .HasColumnName("Statut")
            .IsRequired()
            .HasDefaultValue(DocumentStatut.EnAttente)
            .HasConversion(
                v => v.ToString(),
                v => Enum.Parse<DocumentStatut>(v));

        // Destinataire actuel (Encadrant / Centre)
        builder.Property(d => d.DestinataireActuel)
            .HasColumnName("DestinataireActuel")
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("Encadrant");

        // Vérificateur
        builder.Property(d => d.VerificateurId)
            .HasColumnName("VerificateurId");

        builder.Property(d => d.CommentaireVerificateur)
            .HasColumnName("CommentaireVerificateur")
            .HasMaxLength(2000);

        // Dates
        builder.Property(d => d.DateDepot)
            .HasColumnName("DateDepot")
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(d => d.DateValidation)
            .HasColumnName("DateValidation");

        builder.Property(d => d.DateMiseAJour)
            .HasColumnName("DateMiseAJour");

        // Versioning
        builder.Property(d => d.EstVersionCourante)
            .HasColumnName("EstVersionCourante")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(d => d.DocumentPrecedentId)
            .HasColumnName("DocumentPrecedentId");

        builder.Property(d => d.Version)
            .HasColumnName("Version")
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(d => d.DestinataireActuel)
            .HasColumnName("DestinataireActuel")
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("Encadrant");

        // Index pour les requêtes fréquentes
        builder.HasIndex(d => d.StagiaireId)
            .HasDatabaseName("IX_Document_StagiaireId");

        builder.HasIndex(d => d.CandidatureId)
            .HasDatabaseName("IX_Document_CandidatureId");

        builder.HasIndex(d => d.Statut)
            .HasDatabaseName("IX_Document_Statut");

        builder.HasIndex(d => d.Type)
            .HasDatabaseName("IX_Document_Type");

        builder.HasIndex(d => new { d.StagiaireId, d.Type, d.EstVersionCourante })
            .HasDatabaseName("IX_Document_StagiaireId_Type_Version");

        builder.HasIndex(d => d.EstVersionCourante)
            .HasDatabaseName("IX_Document_EstVersionCourante");

        // Nom de la table
        builder.ToTable("Documents", "dbo");
    }

    private static TypeDocument ParseTypeDocument(string v)
    {
        if (Enum.TryParse<TypeDocument>(v, out var result))
            return result;
        return TypeDocument.CV; // fallback pour les anciennes valeurs supprimées
    }
}
