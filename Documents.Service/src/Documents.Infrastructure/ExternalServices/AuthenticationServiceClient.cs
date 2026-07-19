using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace Documents.Infrastructure.ExternalServices;

/// <summary>
/// Client HTTP vers Authentication.Service
/// </summary>
public class AuthenticationServiceClient : IAuthenticationServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuthenticationServiceClient> _logger;

    public AuthenticationServiceClient(
        HttpClient httpClient,
        ILogger<AuthenticationServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Vérifie si un utilisateur existe et est actif
    /// </summary>
    public async Task<bool> UtilisateurExisteAsync(int utilisateurId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/v1/users/{utilisateurId}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Erreur lors de la vérification de l'existence de l'utilisateur {UtilisateurId}",
                utilisateurId);
            return false;
        }
    }

    /// <summary>
    /// Récupère le rôle d'un utilisateur
    /// </summary>
    public async Task<string?> GetRoleUtilisateurAsync(int utilisateurId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/v1/users/{utilisateurId}");
            if (!response.IsSuccessStatusCode)
                return null;

            var result = await response.Content.ReadFromJsonAsync<UserRoleResponse>();
            return result?.Role;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Erreur lors de la récupération du rôle de l'utilisateur {UtilisateurId}",
                utilisateurId);
            return null;
        }
    }

    // DTO interne pour désérialiser la réponse d'Authentication.Service
    private record UserRoleResponse(string Role);
}
