using GestionDesStagiaires.Web.Models;

namespace GestionDesStagiaires.Web.Services;

/// <summary>
/// Interface du service d'authentification API
/// </summary>
public interface IAuthenticationApiService
{
    /// <summary>
    /// Connecte un utilisateur
    /// </summary>
    Task<ApiResponse<LoginResponse>> LoginAsync(string email, string password);

    /// <summary>
    /// Récupère les informations de l'utilisateur connecté
    /// </summary>
    Task<ApiResponse<CurrentUserInfo>> GetCurrentUserAsync(string token);

    /// <summary>
    /// Vérifie si l'utilisateur est connecté
    /// </summary>
    Task<bool> IsAuthenticatedAsync(string token);

    /// <summary>
    /// Déconnecte l'utilisateur
    /// </summary>
    Task LogoutAsync();
}
