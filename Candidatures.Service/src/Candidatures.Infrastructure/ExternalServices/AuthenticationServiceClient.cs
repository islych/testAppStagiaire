using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Candidatures.Infrastructure.ExternalServices;

/// <summary>
/// Client HTTP pour communiquer avec Authentication.Service
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
    /// Récupère un utilisateur par son ID depuis Authentication.Service
    /// </summary>
    public async Task<UserFromAuthServiceDto?> GetUserByIdAsync(int userId, string token)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/users/{userId}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            _logger.LogInformation("Appel Authentication.Service pour récupérer l'utilisateur {UserId}", userId);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Erreur lors de la récupération de l'utilisateur {UserId}: {Status}", userId, response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var apiResponse = JsonSerializer.Deserialize<AuthServiceApiResponse<UserFromAuthServiceDto>>(json, options);

            if (apiResponse?.Success != true)
            {
                _logger.LogWarning("Authentication.Service a retourné une erreur pour l'utilisateur {UserId}: {Message}", userId, apiResponse?.Message);
                return null;
            }

            _logger.LogInformation("Utilisateur {UserId} récupéré avec succès", userId);
            return apiResponse.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'appel à Authentication.Service pour l'utilisateur {UserId}", userId);
            return null;
        }
    }

    /// <summary>
    /// Récupère plusieurs utilisateurs par batch depuis Authentication.Service
    /// </summary>
    public async Task<IEnumerable<UserFromAuthServiceDto>> GetUsersBatchAsync(List<int> userIds, string token)
    {
        try
        {
            if (userIds == null || userIds.Count == 0)
            {
                _logger.LogWarning("Liste des IDs vide pour batch");
                return Enumerable.Empty<UserFromAuthServiceDto>();
            }

            var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/users/batch")
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(userIds),
                    System.Text.Encoding.UTF8,
                    "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            _logger.LogInformation("Appel Authentication.Service batch pour {Count} utilisateurs", userIds.Count);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Erreur lors de l'appel batch: {Status}", response.StatusCode);
                return Enumerable.Empty<UserFromAuthServiceDto>();
            }

            var json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var apiResponse = JsonSerializer.Deserialize<AuthServiceApiResponse<IEnumerable<UserFromAuthServiceDto>>>(json, options);

            if (apiResponse?.Success != true)
            {
                _logger.LogWarning("Authentication.Service a retourné une erreur pour le batch: {Message}", apiResponse?.Message);
                return Enumerable.Empty<UserFromAuthServiceDto>();
            }

            _logger.LogInformation("Batch récupéré: {Count} utilisateurs", apiResponse.Data?.Count() ?? 0);
            return apiResponse.Data ?? Enumerable.Empty<UserFromAuthServiceDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'appel batch à Authentication.Service");
            return Enumerable.Empty<UserFromAuthServiceDto>();
        }
    }

    /// <summary>
    /// Récupère tous les stagiaires depuis Authentication.Service
    /// </summary>
    public async Task<IEnumerable<UserFromAuthServiceDto>> GetStagiairesAsync(string token)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/users/stagiaires");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            _logger.LogInformation("Appel Authentication.Service pour récupérer tous les stagiaires");

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Erreur lors de la récupération des stagiaires: {Status}", response.StatusCode);
                return Enumerable.Empty<UserFromAuthServiceDto>();
            }

            var json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var apiResponse = JsonSerializer.Deserialize<AuthServiceApiResponse<IEnumerable<UserFromAuthServiceDto>>>(json, options);

            if (apiResponse?.Success != true)
            {
                _logger.LogWarning("Authentication.Service a retourné une erreur pour les stagiaires: {Message}", apiResponse?.Message);
                return Enumerable.Empty<UserFromAuthServiceDto>();
            }

            _logger.LogInformation("Stagiaires récupérés: {Count}", apiResponse.Data?.Count() ?? 0);
            return apiResponse.Data ?? Enumerable.Empty<UserFromAuthServiceDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'appel à Authentication.Service pour les stagiaires");
            return Enumerable.Empty<UserFromAuthServiceDto>();
        }
    }

    /// <summary>
    /// Récupère tous les encadrants depuis Authentication.Service
    /// </summary>
    public async Task<IEnumerable<UserFromAuthServiceDto>> GetEncadrantsAsync(string token)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/users/encadrants");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            _logger.LogInformation("Appel Authentication.Service pour récupérer tous les encadrants");

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Erreur lors de la récupération des encadrants: {Status}", response.StatusCode);
                return Enumerable.Empty<UserFromAuthServiceDto>();
            }

            var json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var apiResponse = JsonSerializer.Deserialize<AuthServiceApiResponse<IEnumerable<UserFromAuthServiceDto>>>(json, options);

            if (apiResponse?.Success != true)
            {
                _logger.LogWarning("Authentication.Service a retourné une erreur pour les encadrants: {Message}", apiResponse?.Message);
                return Enumerable.Empty<UserFromAuthServiceDto>();
            }

            _logger.LogInformation("Encadrants récupérés: {Count}", apiResponse.Data?.Count() ?? 0);
            return apiResponse.Data ?? Enumerable.Empty<UserFromAuthServiceDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'appel à Authentication.Service pour les encadrants");
            return Enumerable.Empty<UserFromAuthServiceDto>();
        }
    }
}
