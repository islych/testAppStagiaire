using Documents.Domain.Enums;

namespace Documents.Application.Commands;

/// <summary>
/// Commande pour uploader un nouveau document
/// </summary>
public record UploadDocumentCommand(
    int StagiaireId,
    Guid? CandidatureId,
    TypeDocument Type,
    string NomFichier,
    string NomFichierStockage,
    string CheminFichier,
    string Extension,
    long TailleFichierOctets,
    string ContentType
);
