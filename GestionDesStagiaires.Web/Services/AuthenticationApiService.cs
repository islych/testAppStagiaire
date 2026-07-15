using GestionDesStagiaires.Web.Models;
using System.Text.Json;

namespace GestionDesStagiaires.Web.Services;

/// <summary>
/// Service d'authentification API
/// </summary>
public class AuthenticationApiService : IAuthenticationApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuthenticationApiService> _logger;

    public AuthenticationApiService(
        HttpClient httpClient,
        ILogger<AuthenticationApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Connecte un utilisateur
    /// </summary>
    public async Task<ApiResponse<LoginResponse>> LoginAsync(string email, string password)
    {
        try
        {
            var loginRequest = new { email, motDePasse = password };
            
            var content = new StringContent(
                JsonSerializer.Serialize(loginRequest),
                System.Text.Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync("/api/v1/auth/login", content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"Erreur de connexion : {response.StatusCode}");
                return new ApiResponse<LoginResponse>
                {
                    Success = false,
                    Message = "Identifiants invalides"
                };
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<LoginResponse>>(
                jsonResponse,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return apiResponse ?? new ApiResponse<LoginResponse> { Success = false, Message = "Erreur de sérialisation" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la connexion");
            return new ApiResponse<LoginResponse>
            {
                Success = false,
                Message = "Erreur de connexion"
            };
        }
    }

    /// <summary>
    /// Récupère les informations de l'utilisateur connecté
    /// </summary>
    public async Task<ApiResponse<CurrentUserInfo>> GetCurrentUserAsync(string token)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/auth/current-user");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"Erreur récupération utilisateur : {response.StatusCode}");
                return new ApiResponse<CurrentUserInfo> { Success = false };
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<CurrentUserInfo>>(
                jsonResponse,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return apiResponse ?? new ApiResponse<CurrentUserInfo> { Success = false };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération de l'utilisateur");
            return new ApiResponse<CurrentUserInfo> { Success = false };
        }
    }

    /// <summary>
    /// Vérifie si l'utilisateur est connecté
    /// </summary>
    public async Task<bool> IsAuthenticatedAsync(string token)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/auth/verify");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Déconnecte l'utilisateur
    /// </summary>
    public async Task LogoutAsync()
    {
        try
        {
            await _httpClient.PostAsync("/api/v1/auth/logout", null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la déconnexion");
        }
    }
}
