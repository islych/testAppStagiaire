using Candidatures.Domain.Enums;

namespace Candidatures.Application.DTOs;

/// <summary>
/// DTO pour le suivi d'une candidature par le stagiaire
/// </summary>
public class CandidatureSuiviDto
{
    /// <summary>
    /// Identifiant de la candidature
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Statut actuel
    /// </summary>
    public CandidatureStatus Statut { get; set; }

    /// <summary>
    /// Date de création
    /// </summary>
    public DateTime DateCreation { get; set; }

    /// <summary>
    /// Date de décision
    /// </summary>
    public DateTime? DateDecision { get; set; }

    /// <summary>
    /// Commentaire (si refus)
    /// </summary>
    public string? Commentaire { get; set; }
}
