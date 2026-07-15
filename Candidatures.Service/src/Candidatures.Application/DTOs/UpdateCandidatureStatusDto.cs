namespace Candidatures.Application.DTOs;

/// <summary>
/// DTO pour mettre à jour le statut d'une candidature
/// </summary>
public class UpdateCandidatureStatusDto
{
    /// <summary>
    /// Identifiant de l'encadrant qui traite
    /// </summary>
    public Guid EncadrantId { get; set; }

    /// <summary>
    /// Commentaire (obligatoire pour refus)
    /// </summary>
    public string? Commentaire { get; set; }
}
