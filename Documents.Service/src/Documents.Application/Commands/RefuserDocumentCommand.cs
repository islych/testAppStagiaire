namespace Documents.Application.Commands;

/// <summary>
/// Commande pour refuser un document avec commentaire obligatoire
/// </summary>
public record RefuserDocumentCommand(
    Guid DocumentId,
    int VerificateurId,
    string CommentaireRefus
);
