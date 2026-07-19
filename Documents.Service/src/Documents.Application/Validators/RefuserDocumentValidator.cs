using Documents.Application.Commands;

namespace Documents.Application.Validators;

/// <summary>
/// Validateur pour le refus d'un document
/// </summary>
public class RefuserDocumentValidator
{
    public ValidationResult Validate(RefuserDocumentCommand command)
    {
        var errors = new List<string>();

        if (command.DocumentId == Guid.Empty)
            errors.Add("L'identifiant du document est requis.");

        if (command.VerificateurId <= 0)
            errors.Add("L'identifiant du vérificateur est requis.");

        if (string.IsNullOrWhiteSpace(command.CommentaireRefus))
            errors.Add("Le commentaire de refus est obligatoire.");
        else if (command.CommentaireRefus.Length < 10)
            errors.Add("Le commentaire de refus doit contenir au moins 10 caractères.");

        return new ValidationResult { IsValid = errors.Count == 0, Errors = errors };
    }
}
