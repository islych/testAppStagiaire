namespace Documents.Application.Validators;

/// <summary>
/// Résultat d'une validation
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
}
