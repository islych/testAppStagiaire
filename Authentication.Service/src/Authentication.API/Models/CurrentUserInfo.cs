namespace Authentication.API.Models;

/// <summary>
/// Informations de l'utilisateur connecté
/// </summary>
public class CurrentUserInfo
{
    /// <summary>
    /// Identifiant unique de l'utilisateur
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Nom de l'utilisateur
    /// </summary>
    public string Nom { get; set; } = string.Empty;

    /// <summary>
    /// Prénom de l'utilisateur
    /// </summary>
    public string Prenom { get; set; } = string.Empty;

    /// <summary>
    /// Email de l'utilisateur
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Rôle de l'utilisateur
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// Statut du compte (actif/inactif)
    /// </summary>
    public bool Statut { get; set; }

    /// <summary>
    /// Date de création du compte
    /// </summary>
    public DateTime DateCreation { get; set; }
}
