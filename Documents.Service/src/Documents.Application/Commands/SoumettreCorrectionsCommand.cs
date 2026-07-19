using Documents.Domain.Enums;

namespace Documents.Application.Commands;

/// <summary>
/// Commande pour soumettre une nouvelle version d'un document refusé
/// </summary>
public record SoumettreCorrectionsCommand(
    Guid DocumentOriginalId,
    int StagiaireId,
    TypeDocument Type,
    string NomFichier,
    string NomFichierStockage,
    string CheminFichier,
    string Extension,
    long TailleFichierOctets,
    string ContentType
);
