using Documents.Domain.Enums;

namespace Documents.Domain.Entities;

/// <summary>
/// Entité représentant un document administratif déposé par un stagiaire
/// </summary>
public class Document
{
    /// <summary>
    /// Identifiant unique du document
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identifiant du stagiaire propriétaire du document (int - matching Authentication.Service)
    /// </summary>
    public int StagiaireId { get; set; }

    /// <summary>
    /// Identifiant de la candidature associée (optionnel)
    /// </summary>
    public Guid? CandidatureId { get; set; }

    /// <summary>
    /// Type du document (CV, Convention, Assurance, CIN, etc.)
    /// </summary>
    public TypeDocument Type { get; set; }

    /// <summary>
    /// Nom original du fichier tel qu'uploadé par le stagiaire
    /// </summary>
    public string NomFichier { get; set; } = string.Empty;

    /// <summary>
    /// Nom du fichier stocké sur le serveur (unique, généré)
    /// </summary>
    public string NomFichierStockage { get; set; } = string.Empty;

    /// <summary>
    /// Chemin ou URL du fichier sur le serveur
    /// </summary>
    public string CheminFichier { get; set; } = string.Empty;

    /// <summary>
    /// Extension du fichier (ex: .pdf, .jpg, .png)
    /// </summary>
    public string Extension { get; set; } = string.Empty;

    /// <summary>
    /// Taille du fichier en octets
    /// </summary>
    public long TailleFichierOctets { get; set; }

    /// <summary>
    /// Type MIME du fichier (ex: application/pdf)
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// Statut actuel du document dans le cycle de validation
    /// </summary>
    public DocumentStatut Statut { get; set; } = DocumentStatut.EnAttente;

    /// <summary>
    /// Identifiant du vérificateur (encadrant / RH / Centre) qui a traité le document
    /// </summary>
    public int? VerificateurId { get; set; }

    /// <summary>
    /// Commentaire du vérificateur (motif de refus ou note de validation)
    /// </summary>
    public string? CommentaireVerificateur { get; set; }

    /// <summary>
    /// Date de dépôt du document
    /// </summary>
    public DateTime DateDepot { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date de validation ou de refus du document
    /// </summary>
    public DateTime? DateValidation { get; set; }

    /// <summary>
    /// Date de la dernière mise à jour
    /// </summary>
    public DateTime? DateMiseAJour { get; set; }

    /// <summary>
    /// Indique si ce document est la version courante (après correction)
    /// </summary>
    public bool EstVersionCourante { get; set; } = true;

    /// <summary>
    /// Référence vers l'ID du document précédent (en cas de correction)
    /// </summary>
    public Guid? DocumentPrecedentId { get; set; }

    /// <summary>
    /// Numéro de version du document (1, 2, 3… après corrections)
    /// </summary>
    public int Version { get; set; } = 1;
}
