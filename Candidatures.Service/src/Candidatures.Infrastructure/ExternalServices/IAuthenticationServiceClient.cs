namespace Candidatures.Infrastructure.ExternalServices;

/// <summary>
/// Interface pour communiquer avec Authentication.Service
/// </summary>
public interface IAuthenticationServiceClient
{
    /// <summary>
    /// Récupère un utilisateur par son ID
    /// </summary>
    /// <param name="userId">ID de l'utilisateur (int)</param>
    /// <param name="token">JWT token pour l'authentification</param>
    /// <returns>Données de l'utilisateur ou null si non trouvé</returns>
    Task<UserFromAuthServiceDto?> GetUserByIdAsync(int userId, string token);

    /// <summary>
    /// Récupère plusieurs utilisateurs par leurs IDs en une seule requête
    /// </summary>
    /// <param name="userIds">Liste des IDs d'utilisateurs (int)</param>
    /// <param name="token">JWT token pour l'authentification</param>
    /// <returns>Collection des utilisateurs trouvés</returns>
    Task<IEnumerable<UserFromAuthServiceDto>> GetUsersBatchAsync(List<int> userIds, string token);

    /// <summary>
    /// Récupère tous les stagiaires
    /// </summary>
    /// <param name="token">JWT token pour l'authentification</param>
    /// <returns>Collection de tous les stagiaires</returns>
    Task<IEnumerable<UserFromAuthServiceDto>> GetStagiairesAsync(string token);

    /// <summary>
    /// Récupère tous les encadrants
    /// </summary>
    /// <param name="token">JWT token pour l'authentification</param>
    /// <returns>Collection de tous les encadrants</returns>
    Task<IEnumerable<UserFromAuthServiceDto>> GetEncadrantsAsync(string token);
}

/// <summary>
/// DTO pour les informations utilisateur reçues d'Authentication.Service
/// </summary>
public class UserFromAuthServiceDto
{
    /// <summary>
    /// Identifiant unique (int - matching Authentication.Service)
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

/// <summary>
/// Réponse générique d'Authentication.Service
/// </summary>
public class AuthServiceApiResponse<T>
{
    /// <summary>
    /// Indicateur de succès
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Message descriptif
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Données retournées
    /// </summary>
    public T? Data { get; set; }
}
