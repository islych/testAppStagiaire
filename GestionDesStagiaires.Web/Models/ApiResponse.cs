namespace GestionDesStagiaires.Web.Models;

/// <summary>
/// Réponse générique de l'API
/// </summary>
public class ApiResponse<T>
{
    /// <summary>
    /// Indique si l'opération a réussi
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Message de réponse
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Données de la réponse
    /// </summary>
    public T? Data { get; set; }
}
