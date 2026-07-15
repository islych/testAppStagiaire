namespace Authentication.API.Models;

/// <summary>
/// Réponse de connexion
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// Identifiant unique de l'utilisateur
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Email de l'utilisateur
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Prénom
    /// </summary>
    public string Prenom { get; set; } = string.Empty;

    /// <summary>
    /// Nom
    /// </summary>
    public string Nom { get; set; } = string.Empty;

    /// <summary>
    /// Rôle de l'utilisateur
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// JWT Token
    /// </summary>
    public string Token { get; set; } = string.Empty;
}
