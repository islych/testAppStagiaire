using Documents.Domain.Enums;

namespace Documents.Application.DTOs;

/// <summary>
/// DTO de réponse représentant un document
/// </summary>
public class DocumentDto
{
    public Guid Id { get; set; }
    public int StagiaireId { get; set; }
    public Guid? CandidatureId { get; set; }
    public TypeDocument Type { get; set; }
    public string TypeLibelle { get; set; } = string.Empty;
    public string NomFichier { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public long TailleFichierOctets { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public DocumentStatut Statut { get; set; }
    public string StatutLibelle { get; set; } = string.Empty;
    public int? VerificateurId { get; set; }
    public string? CommentaireVerificateur { get; set; }
    public DateTime DateDepot { get; set; }
    public DateTime? DateValidation { get; set; }
    public DateTime? DateMiseAJour { get; set; }
    public bool EstVersionCourante { get; set; }
    public Guid? DocumentPrecedentId { get; set; }
    public int Version { get; set; }
}
