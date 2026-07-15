namespace GestionDesStagiaires.Web.Models;

/// <summary>
/// Requête d'enregistrement pour un nouveau stagiaire
/// </summary>
public class RegisterRequest
{
    /// <summary>
    /// Nom de l'utilisateur
    /// </summary>
    public string Nom { get; set; } = string.Empty;

    /// <summary>
    /// Prénom de l'utilisateur
    /// </summary>
    public string Prenom { get; set; } = string.Empty;

    /// <summary>
    /// Email unique
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Mot de passe (minimum 8 caractères)
    /// </summary>
    public string MotDePasse { get; set; } = string.Empty;

    /// <summary>
    /// Confirmation du mot de passe
    /// </summary>
    public string ConfirmMotDePasse { get; set; } = string.Empty;
}
