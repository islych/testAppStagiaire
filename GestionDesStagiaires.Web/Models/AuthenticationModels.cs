namespace GestionDesStagiaires.Web.Models;

/// <summary>
/// Requête de connexion
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// Email de l'utilisateur
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Mot de passe
    /// </summary>
    public string MotDePasse { get; set; } = string.Empty;
}

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
    /// Rôle de l'utilisateur (Stagiaire, Encadrant, Direction, etc.)
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// JWT Token
    /// </summary>
    public string Token { get; set; } = string.Empty;
}

/// <summary>
/// Informations de l'utilisateur connecté
/// </summary>
public class CurrentUserInfo
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string FullName => $"{Prenom} {Nom}";
}
