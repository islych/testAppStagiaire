using Candidatures.Application.Commands;

namespace Candidatures.Application.Validators;

/// <summary>
/// Validateur pour l'acceptation d'une candidature
/// </summary>
public class AcceptCandidatureValidator
{
    /// <summary>
    /// Valide la commande d'acceptation
    /// </summary>
    public ValidationResult Validate(AcceptCandidatureCommand command)
    {
        var errors = new List<string>();

        if (command.CandidatureId == Guid.Empty)
            errors.Add("L'identifiant de la candidature est requis.");

        if (command.EncadrantId <= 0)
            errors.Add("L'identifiant de l'encadrant est requis.");

        return new ValidationResult { IsValid = errors.Count == 0, Errors = errors };
    }
}
