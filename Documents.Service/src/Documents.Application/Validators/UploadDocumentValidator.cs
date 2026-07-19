using Documents.Application.Commands;

namespace Documents.Application.Validators;

/// <summary>
/// Validateur pour l'upload d'un document
/// </summary>
public class UploadDocumentValidator
{
    private static readonly string[] ExtensionsAutorisees = { ".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx" };
    private const long TailleMaxOctets = 10 * 1024 * 1024; // 10 Mo

    public ValidationResult Validate(UploadDocumentCommand command)
    {
        var errors = new List<string>();

        if (command.StagiaireId <= 0)
            errors.Add("L'identifiant du stagiaire est requis.");

        if (string.IsNullOrWhiteSpace(command.NomFichier))
            errors.Add("Le nom du fichier est requis.");

        if (string.IsNullOrWhiteSpace(command.CheminFichier))
            errors.Add("Le chemin du fichier est requis.");

        if (string.IsNullOrWhiteSpace(command.Extension))
            errors.Add("L'extension du fichier est requise.");
        else if (!ExtensionsAutorisees.Contains(command.Extension.ToLower()))
            errors.Add($"L'extension '{command.Extension}' n'est pas autorisée. Extensions acceptées : {string.Join(", ", ExtensionsAutorisees)}.");

        if (command.TailleFichierOctets <= 0)
            errors.Add("Le fichier est vide.");
        else if (command.TailleFichierOctets > TailleMaxOctets)
            errors.Add($"Le fichier dépasse la taille maximale autorisée de {TailleMaxOctets / (1024 * 1024)} Mo.");

        return new ValidationResult { IsValid = errors.Count == 0, Errors = errors };
    }
}
