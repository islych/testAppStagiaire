using Documents.Domain.Enums;

namespace Documents.Application.DTOs;

/// <summary>
/// DTO de requête pour l'upload d'un document
/// Reçu en multipart/form-data depuis le contrôleur
/// </summary>
public class UploadDocumentDto
{
    /// <summary>ID du stagiaire propriétaire du document</summary>
    public int StagiaireId { get; set; }

    /// <summary>ID de la candidature associée (optionnel)</summary>
    public Guid? CandidatureId { get; set; }

    /// <summary>Type du document</summary>
    public TypeDocument Type { get; set; }
}
