namespace Documents.Application.DTOs;

/// <summary>
/// DTO pour la demande de correction d'un document par un encadrant
/// </summary>
public class DemanderCorrectionDto
{
    /// <summary>Commentaire explicatif obligatoire pour le stagiaire</summary>
    public string Commentaire { get; set; } = string.Empty;
}
