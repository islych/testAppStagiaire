namespace Documents.Application.DTOs;

/// <summary>
/// Enveloppe de réponse standardisée pour toutes les APIs
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
}
