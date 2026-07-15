using GestionDesStagiaires.Web.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GestionDesStagiaires.Web.Services;

/// <summary>
/// Service des candidatures API
/// </summary>
public class CandidaturesApiService : ICandidaturesApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CandidaturesApiService> _logger;
    
    // Options de sérialisation JSON avec support des enums
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    public CandidaturesApiService(
        HttpClient httpClient,
        ILogger<CandidaturesApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Crée une nouvelle candidature avec tous les détails (période, dates, niveau, école, etc.)
    /// </summary>
    public async Task<ApiResponse<CandidatureViewModel>> CreateCandidatureAsync(
        CreateCandidatureViewModel candidature,
        string token)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/candidatures");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var json = JsonSerializer.Serialize(candidature, JsonOptions);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            request.Content = content;
            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"Erreur création candidature : {response.StatusCode}");
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning($"Erreur détails : {errorContent}");
                
                return new ApiResponse<CandidatureViewModel>
                {
                    Success = false,
                    Message = "Erreur lors de la création"
                };
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<CandidatureViewModel>>(
                jsonResponse,
                JsonOptions);

            return apiResponse ?? new ApiResponse<CandidatureViewModel> { Success = false };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la création de candidature");
            return new ApiResponse<CandidatureViewModel>
            {
                Success = false,
                Message = "Erreur serveur"
            };
        }
    }

    /// <summary>
    /// Récupère une candidature par son ID
    /// </summary>
    public async Task<ApiResponse<CandidatureViewModel>> GetCandidatureByIdAsync(Guid id, string token)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/candidatures/{id}");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"Candidature {id} non trouvée");
                return new ApiResponse<CandidatureViewModel> { Success = false };
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<CandidatureViewModel>>(
                jsonResponse,
                JsonOptions);

            return apiResponse ?? new ApiResponse<CandidatureViewModel> { Success = false };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erreur lors de la récupération de la candidature {id}");
            return new ApiResponse<CandidatureViewModel> { Success = false };
        }
    }

    /// <summary>
    /// Récupère toutes les candidatures
    /// </summary>
    public async Task<ApiResponse<IEnumerable<CandidatureViewModel>>> GetAllCandidaturesAsync(string token)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/candidatures");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"Erreur récupération candidatures : {response.StatusCode}");
                return new ApiResponse<IEnumerable<CandidatureViewModel>> { Success = false };
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<IEnumerable<CandidatureViewModel>>>(
                jsonResponse,
                JsonOptions);

            return apiResponse ?? new ApiResponse<IEnumerable<CandidatureViewModel>> { Success = false };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des candidatures");
            return new ApiResponse<IEnumerable<CandidatureViewModel>> { Success = false };
        }
    }

    /// <summary>
    /// Récupère les candidatures du stagiaire connecté
    /// </summary>
    public async Task<ApiResponse<IEnumerable<CandidatureViewModel>>> GetMyCandidaturesAsync(string token)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/candidatures/me/candidatures");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"Erreur récupération mes candidatures : {response.StatusCode}");
                return new ApiResponse<IEnumerable<CandidatureViewModel>> { Success = false };
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<IEnumerable<CandidatureViewModel>>>(
                jsonResponse,
                JsonOptions);

            return apiResponse ?? new ApiResponse<IEnumerable<CandidatureViewModel>> { Success = false };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération de mes candidatures");
            return new ApiResponse<IEnumerable<CandidatureViewModel>> { Success = false };
        }
    }

    /// <summary>
    /// Récupère le suivi d'une candidature
    /// </summary>
    public async Task<ApiResponse<CandidatureSuiviViewModel>> GetCandidatureSuiviAsync(Guid id, string token)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/candidatures/{id}/suivi");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"Suivi candidature {id} non trouvé");
                return new ApiResponse<CandidatureSuiviViewModel> { Success = false };
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<CandidatureSuiviViewModel>>(
                jsonResponse,
                JsonOptions);

            return apiResponse ?? new ApiResponse<CandidatureSuiviViewModel> { Success = false };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erreur lors de la récupération du suivi {id}");
            return new ApiResponse<CandidatureSuiviViewModel> { Success = false };
        }
    }

    /// <summary>
    /// Accepte une candidature
    /// </summary>
    public async Task<ApiResponse<CandidatureViewModel>> AcceptCandidatureAsync(Guid id, string token)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/candidatures/{id}/accept");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"Erreur acceptation candidature {id} : {response.StatusCode}");
                return new ApiResponse<CandidatureViewModel> { Success = false };
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<CandidatureViewModel>>(
                jsonResponse,
                JsonOptions);

            return apiResponse ?? new ApiResponse<CandidatureViewModel> { Success = false };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erreur lors de l'acceptation de la candidature {id}");
            return new ApiResponse<CandidatureViewModel> { Success = false };
        }
    }

    /// <summary>
    /// Refuse une candidature
    /// </summary>
    public async Task<ApiResponse<CandidatureViewModel>> RejectCandidatureAsync(Guid id, string motif, string token)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/candidatures/{id}/reject");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var rejectData = new { commentaire = motif };
            var content = new StringContent(
                JsonSerializer.Serialize(rejectData, JsonOptions),
                System.Text.Encoding.UTF8,
                "application/json");

            request.Content = content;
            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"Erreur refus candidature {id} : {response.StatusCode}");
                return new ApiResponse<CandidatureViewModel> { Success = false };
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<CandidatureViewModel>>(
                jsonResponse,
                JsonOptions);

            return apiResponse ?? new ApiResponse<CandidatureViewModel> { Success = false };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erreur lors du refus de la candidature {id}");
            return new ApiResponse<CandidatureViewModel> { Success = false };
        }
    }
}
