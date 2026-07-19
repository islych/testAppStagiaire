namespace Documents.Application.DTOs;

/// <summary>
/// DTO de requête pour refuser un document
/// </summary>
public class RefuserDocumentDto
{
    /// <summary>Motif du refus — obligatoire</summary>
    public string CommentaireRefus { get; set; } = string.Empty;
}
