using GestionDesStagiaires.Web.Models;
using System.Text.Json;

namespace GestionDesStagiaires.Web.Services;

/// <summary>
/// Service d'accès aux données maîtres via l'API Candidatures.Service
/// </summary>
public class MasterDataApiService : IMasterDataApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<MasterDataApiService> _logger;

    public MasterDataApiService(
        HttpClient httpClient,
        ILogger<MasterDataApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Récupère la liste de tous les départements
    /// </summary>
    public async Task<ApiResponse<IEnumerable<DepartementOptionResponse>>> GetDepartementsAsync(string token)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/masterdata/departements");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"Erreur récupération départements : {response.StatusCode}");
                return new ApiResponse<IEnumerable<DepartementOptionResponse>>
                {
                    Success = false,
                    Message = "Impossible de récupérer les départements"
                };
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<IEnumerable<DepartementOptionResponse>>>(
                jsonResponse,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return apiResponse ?? new ApiResponse<IEnumerable<DepartementOptionResponse>> { Success = false };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des départements");
            return new ApiResponse<IEnumerable<DepartementOptionResponse>>
            {
                Success = false,
                Message = "Erreur serveur"
            };
        }
    }

    /// <summary>
    /// Récupère les spécialités d'un département
    /// </summary>
    public async Task<ApiResponse<IEnumerable<SpecialiteOptionResponse>>> GetSpecialitesByDepartementAsync(int departementId, string token)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/masterdata/specialites/{departementId}");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"Erreur récupération spécialités : {response.StatusCode}");
                return new ApiResponse<IEnumerable<SpecialiteOptionResponse>>
                {
                    Success = false,
                    Message = "Impossible de récupérer les spécialités"
                };
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<IEnumerable<SpecialiteOptionResponse>>>(
                jsonResponse,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return apiResponse ?? new ApiResponse<IEnumerable<SpecialiteOptionResponse>> { Success = false };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erreur lors de la récupération des spécialités du département {departementId}");
            return new ApiResponse<IEnumerable<SpecialiteOptionResponse>>
            {
                Success = false,
                Message = "Erreur serveur"
            };
        }
    }

    /// <summary>
    /// Récupère un département par son ID
    /// </summary>
    public async Task<ApiResponse<DepartementOptionResponse>> GetDepartementByIdAsync(int departementId, string token)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/masterdata/departements/{departementId}");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"Département {departementId} non trouvé");
                return new ApiResponse<DepartementOptionResponse> { Success = false };
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<DepartementOptionResponse>>(
                jsonResponse,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return apiResponse ?? new ApiResponse<DepartementOptionResponse> { Success = false };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erreur lors de la récupération du département {departementId}");
            return new ApiResponse<DepartementOptionResponse> { Success = false };
        }
    }

    /// <summary>
    /// Récupère une spécialité par son ID
    /// </summary>
    public async Task<ApiResponse<SpecialiteOptionResponse>> GetSpecialiteByIdAsync(int specialiteId, string token)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/masterdata/specialites/{specialiteId}");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"Spécialité {specialiteId} non trouvée");
                return new ApiResponse<SpecialiteOptionResponse> { Success = false };
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<SpecialiteOptionResponse>>(
                jsonResponse,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return apiResponse ?? new ApiResponse<SpecialiteOptionResponse> { Success = false };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erreur lors de la récupération de la spécialité {specialiteId}");
            return new ApiResponse<SpecialiteOptionResponse> { Success = false };
        }
    }
}
