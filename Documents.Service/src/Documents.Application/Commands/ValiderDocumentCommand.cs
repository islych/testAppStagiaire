namespace Documents.Application.Commands;

/// <summary>
/// Commande pour valider un document (Encadrant / RH / Centre)
/// </summary>
public record ValiderDocumentCommand(
    Guid DocumentId,
    int VerificateurId,
    string? Commentaire
);
