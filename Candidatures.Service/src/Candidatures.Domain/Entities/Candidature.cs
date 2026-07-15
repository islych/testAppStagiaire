using Candidatures.Domain.Enums;

namespace Candidatures.Domain.Entities;

/// <summary>
/// Entité représentant une candidature de stagiaire
/// </summary>
public class Candidature
{
    /// <summary>
    /// Identifiant unique de la candidature
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identifiant du stagiaire qui a déposé la candidature (int - matching Authentication.Service)
    /// </summary>
    public int StagiaireId { get; set; }

    /// <summary>
    /// Identifiant du département pour lequel le stagiaire candidate
    /// </summary>
    public int DepartementId { get; set; }

    /// <summary>
    /// Identifiant de la spécialité choisie
    /// </summary>
    public int SpecialiteId { get; set; }

    /// <summary>
    /// Nom du fichier CV uploadé
    /// </summary>
    public string CvFileName { get; set; } = string.Empty;

    /// <summary>
    /// Chemin ou URL du fichier CV
    /// </summary>
    public string CvPath { get; set; } = string.Empty;

    /// <summary>
    /// Durée du stage souhaitée en mois (1-6 mois)
    /// </summary>
    public int DureeStageMois { get; set; } = 3;

    /// <summary>
    /// Date de début du stage souhaitée
    /// </summary>
    public DateTime DateDebut { get; set; }

    /// <summary>
    /// Date de fin du stage souhaitée
    /// </summary>
    public DateTime DateFin { get; set; }

    /// <summary>
    /// Niveau d'études du stagiaire
    /// </summary>
    public NiveauEtude NiveauEtude { get; set; } = NiveauEtude.BacPlus3;

    /// <summary>
    /// Nom de l'école/université d'origine
    /// </summary>
    public string Ecole { get; set; } = string.Empty;

    /// <summary>
    /// Lettre de motivation (texte)
    /// </summary>
    public string LettreMotivation { get; set; } = string.Empty;

    /// <summary>
    /// Statut actuel de la candidature
    /// </summary>
    public CandidatureStatus Statut { get; set; } = CandidatureStatus.EnAttente;

    /// <summary>
    /// Identifiant de l'encadrant (int - matching Authentication.Service, null si pas encore assigné)
    /// </summary>
    public int? EncadrantId { get; set; }

    /// <summary>
    /// Commentaire de l'encadrant (ex: motif du refus)
    /// </summary>
    public string? Commentaire { get; set; }

    /// <summary>
    /// Date de création de la candidature
    /// </summary>
    public DateTime DateCreation { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date de la dernière mise à jour
    /// </summary>
    public DateTime? DateMiseAJour { get; set; }

    /// <summary>
    /// Date de décision de l'encadrant
    /// </summary>
    public DateTime? DateDecision { get; set; }

    /// <summary>
    /// Indique si la candidature a été transmise à la Direction
    /// </summary>
    public bool TransmisADirection { get; set; } = false;

    /// <summary>
    /// Date de transmission à la Direction
    /// </summary>
    public DateTime? DateTransmissionDirection { get; set; }
}
