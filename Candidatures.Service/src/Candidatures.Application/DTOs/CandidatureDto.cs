using Candidatures.Domain.Enums;

namespace Candidatures.Application.DTOs;

/// <summary>
/// DTO pour afficher une candidature
/// </summary>
public class CandidatureDto
{
    /// <summary>
    /// Identifiant unique
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identifiant du stagiaire (int - matching Authentication.Service)
    /// </summary>
    public int StagiaireId { get; set; }

    /// <summary>
    /// Nom du stagiaire (enrichi depuis Authentication.Service)
    /// </summary>
    public string StagiaireNom { get; set; } = string.Empty;

    /// <summary>
    /// Prénom du stagiaire (enrichi depuis Authentication.Service)
    /// </summary>
    public string StagiairePrenom { get; set; } = string.Empty;

    /// <summary>
    /// Email du stagiaire (enrichi depuis Authentication.Service)
    /// </summary>
    public string StagiaireEmail { get; set; } = string.Empty;

    /// <summary>
    /// Identifiant du département
    /// </summary>
    public int DepartementId { get; set; }

    /// <summary>
    /// Nom du département
    /// </summary>
    public string DepartementNom { get; set; } = string.Empty;

    /// <summary>
    /// Identifiant de la spécialité
    /// </summary>
    public int SpecialiteId { get; set; }

    /// <summary>
    /// Nom de la spécialité
    /// </summary>
    public string SpecialiteNom { get; set; } = string.Empty;

    /// <summary>
    /// Durée du stage en mois (1-6 mois)
    /// </summary>
    public int DureeStageMois { get; set; }

    /// <summary>
    /// Date de début
    /// </summary>
    public DateTime DateDebut { get; set; }

    /// <summary>
    /// Date de fin
    /// </summary>
    public DateTime DateFin { get; set; }

    /// <summary>
    /// Niveau d'études
    /// </summary>
    public NiveauEtude NiveauEtude { get; set; }

    /// <summary>
    /// Nom de l'école
    /// </summary>
    public string Ecole { get; set; } = string.Empty;

    /// <summary>
    /// Lettre de motivation
    /// </summary>
    public string LettreMotivation { get; set; } = string.Empty;

    /// <summary>
    /// Nom du fichier CV
    /// </summary>
    public string CvFileName { get; set; } = string.Empty;

    /// <summary>
    /// Chemin du fichier CV
    /// </summary>
    public string CvPath { get; set; } = string.Empty;

    /// <summary>
    /// Statut actuel
    /// </summary>
    public CandidatureStatus Statut { get; set; }

    /// <summary>
    /// Identifiant de l'encadrant (int - matching Authentication.Service)
    /// </summary>
    public int? EncadrantId { get; set; }

    /// <summary>
    /// Nom de l'encadrant (enrichi depuis Authentication.Service)
    /// </summary>
    public string? EncadrantNom { get; set; }

    /// <summary>
    /// Prénom de l'encadrant (enrichi depuis Authentication.Service)
    /// </summary>
    public string? EncadrantPrenom { get; set; }

    /// <summary>
    /// Email de l'encadrant (enrichi depuis Authentication.Service)
    /// </summary>
    public string? EncadrantEmail { get; set; }

    /// <summary>
    /// Commentaire de l'encadrant
    /// </summary>
    public string? Commentaire { get; set; }

    /// <summary>
    /// Date de création
    /// </summary>
    public DateTime DateCreation { get; set; }

    /// <summary>
    /// Date de mise à jour
    /// </summary>
    public DateTime? DateMiseAJour { get; set; }

    /// <summary>
    /// Date de décision
    /// </summary>
    public DateTime? DateDecision { get; set; }

    /// <summary>
    /// Si transmis à la Direction
    /// </summary>
    public bool TransmisADirection { get; set; }

    /// <summary>
    /// Date de transmission à la Direction
    /// </summary>
    public DateTime? DateTransmissionDirection { get; set; }
}
