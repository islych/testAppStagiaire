using Documents.Application.Commands;

namespace Documents.Application.Validators;

/// <summary>
/// Validateur pour la validation d'un document
/// </summary>
public class ValiderDocumentValidator
{
    public ValidationResult Validate(ValiderDocumentCommand command)
    {
        var errors = new List<string>();

        if (command.DocumentId == Guid.Empty)
            errors.Add("L'identifiant du document est requis.");

        if (command.VerificateurId <= 0)
            errors.Add("L'identifiant du vérificateur est requis.");

        return new ValidationResult { IsValid = errors.Count == 0, Errors = errors };
    }
}
