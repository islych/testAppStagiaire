namespace Documents.Application.DTOs;

/// <summary>
/// DTO de requête pour valider un document
/// </summary>
public class ValiderDocumentDto
{
    /// <summary>Commentaire optionnel du vérificateur</summary>
    public string? Commentaire { get; set; }
}
