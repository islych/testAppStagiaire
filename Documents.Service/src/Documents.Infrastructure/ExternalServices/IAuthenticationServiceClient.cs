namespace Documents.Infrastructure.ExternalServices;

/// <summary>
/// Contrat du client HTTP vers Authentication.Service
/// </summary>
public interface IAuthenticationServiceClient
{
    /// <summary>
    /// Vérifie si un utilisateur existe et est actif dans Authentication.Service
    /// </summary>
    Task<bool> UtilisateurExisteAsync(int utilisateurId);

    /// <summary>
    /// Récupère le rôle d'un utilisateur depuis Authentication.Service
    /// </summary>
    Task<string?> GetRoleUtilisateurAsync(int utilisateurId);
}
