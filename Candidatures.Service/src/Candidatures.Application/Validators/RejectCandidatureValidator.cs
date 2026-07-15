using Candidatures.Application.Commands;

namespace Candidatures.Application.Validators;

/// <summary>
/// Validateur pour le refus d'une candidature
/// </summary>
public class RejectCandidatureValidator
{
    /// <summary>
    /// Valide la commande de refus
    /// </summary>
    public ValidationResult Validate(RejectCandidatureCommand command)
    {
        var errors = new List<string>();

        if (command.CandidatureId == Guid.Empty)
            errors.Add("L'identifiant de la candidature est requis.");

        if (command.EncadrantId <= 0)
            errors.Add("L'identifiant de l'encadrant est requis.");

        if (string.IsNullOrWhiteSpace(command.Commentaire))
            errors.Add("Un commentaire (motif du refus) est obligatoire.");

        if (command.Commentaire?.Length < 5)
            errors.Add("Le commentaire doit contenir au moins 5 caractères.");

        return new ValidationResult { IsValid = errors.Count == 0, Errors = errors };
    }
}
