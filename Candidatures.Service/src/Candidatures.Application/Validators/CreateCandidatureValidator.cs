using Candidatures.Application.Commands;

namespace Candidatures.Application.Validators;

/// <summary>
/// Validateur pour la création d'une candidature
/// </summary>
public class CreateCandidatureValidator
{
    /// <summary>
    /// Valide la commande de création
    /// </summary>
    public ValidationResult Validate(CreateCandidatureCommand command)
    {
        var errors = new List<string>();

        if (command.StagiaireId <= 0)
            errors.Add("L'identifiant du stagiaire est requis.");

        if (command.DepartementId <= 0)
            errors.Add("L'identifiant du département est requis.");

        if (command.SpecialiteId <= 0)
            errors.Add("L'identifiant de la spécialité est requis.");

        if (string.IsNullOrWhiteSpace(command.CvFileName))
            errors.Add("Le nom du fichier CV est requis.");

        if (string.IsNullOrWhiteSpace(command.CvPath))
            errors.Add("Le chemin du fichier CV est requis.");

        return new ValidationResult { IsValid = errors.Count == 0, Errors = errors };
    }
}

/// <summary>
/// Résultat de validation
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
}
