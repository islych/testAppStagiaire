using GestionDesStagiaires.Web.Models;
using System.Text.Json;

namespace GestionDesStagiaires.Web.Services;

/// <summary>
/// Service de gestion des utilisateurs
/// </summary>
public interface IUserManagementService
{
    Task<List<UserDto>> GetAllUsersAsync(string token);
    Task<List<RoleDto>> GetRolesAsync(string token);
    Task<ApiResponse<string>> CreateUserAsync(CreateUserRequest request, string token);
    Task<ApiResponse<string>> ToggleUserStatusAsync(int userId, string token);
    Task<ApiResponse<string>> DeleteUserAsync(int userId, string token);
}

public class UserManagementService : IUserManagementService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UserManagementService> _logger;

    public UserManagementService(HttpClient httpClient, ILogger<UserManagementService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Récupère tous les utilisateurs
    /// </summary>
    public async Task<List<UserDto>> GetAllUsersAsync(string token)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/admin/users");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"Erreur récupération utilisateurs : {response.StatusCode}");
                return new List<UserDto>();
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<UserDto>>>(
                jsonResponse,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return apiResponse?.Data ?? new List<UserDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des utilisateurs");
            return new List<UserDto>();
        }
    }

    /// <summary>
    /// Récupère tous les rôles
    /// </summary>
    public async Task<List<RoleDto>> GetRolesAsync(string token)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/admin/roles");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"Erreur récupération rôles : {response.StatusCode}");
                return new List<RoleDto>();
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<RoleDto>>>(
                jsonResponse,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return apiResponse?.Data ?? new List<RoleDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des rôles");
            return new List<RoleDto>();
        }
    }

    /// <summary>
    /// Crée un nouvel utilisateur
    /// </summary>
    public async Task<ApiResponse<string>> CreateUserAsync(CreateUserRequest request, string token)
    {
        try
        {
            var content = new StringContent(
                JsonSerializer.Serialize(request),
                System.Text.Encoding.UTF8,
                "application/json");

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/admin/create-user")
            {
                Content = content
            };
            httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(httpRequest);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"Erreur création utilisateur : {response.StatusCode}");
                return new ApiResponse<string>
                {
                    Success = false,
                    Message = "Erreur lors de la création de l'utilisateur"
                };
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(
                jsonResponse,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return apiResponse ?? new ApiResponse<string> { Success = false, Message = "Erreur de sérialisation" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la création de l'utilisateur");
            return new ApiResponse<string> { Success = false, Message = "Une erreur est survenue" };
        }
    }

    /// <summary>
    /// Bascule le statut d'un utilisateur
    /// </summary>
    public async Task<ApiResponse<string>> ToggleUserStatusAsync(int userId, string token)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/admin/toggle-status/{userId}");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"Erreur basculement statut : {response.StatusCode}");
                return new ApiResponse<string> { Success = false, Message = "Erreur lors du basculement du statut" };
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(
                jsonResponse,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return apiResponse ?? new ApiResponse<string> { Success = false };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du basculement du statut");
            return new ApiResponse<string> { Success = false };
        }
    }

    /// <summary>
    /// Supprime un utilisateur
    /// </summary>
    public async Task<ApiResponse<string>> DeleteUserAsync(int userId, string token)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/admin/delete-user/{userId}");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"Erreur suppression utilisateur : {response.StatusCode}");
                return new ApiResponse<string> { Success = false, Message = "Erreur lors de la suppression" };
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(
                jsonResponse,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return apiResponse ?? new ApiResponse<string> { Success = false };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suppression");
            return new ApiResponse<string> { Success = false };
        }
    }
}
