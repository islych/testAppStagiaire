using GestionDesStagiaires.Web.Models;
using System.Text.Json;

namespace GestionDesStagiaires.Web.Services;

/// <summary>
/// Service d'enregistrement des utilisateurs
/// </summary>
public interface IRegistrationService
{
    Task<ApiResponse<string>> RegisterAsync(RegisterRequest request);
}

public class RegistrationService : IRegistrationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<RegistrationService> _logger;

    public RegistrationService(HttpClient httpClient, ILogger<RegistrationService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Enregistre un nouveau stagiaire
    /// </summary>
    public async Task<ApiResponse<string>> RegisterAsync(RegisterRequest request)
    {
        try
        {
            var content = new StringContent(
                JsonSerializer.Serialize(request),
                System.Text.Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync("/api/v1/auth/register", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning($"Erreur enregistrement : {response.StatusCode} - {errorContent}");
                
                try
                {
                    var errorResponse = JsonSerializer.Deserialize<ApiResponse<string>>(
                        errorContent,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    
                    return errorResponse ?? new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Erreur lors de l'enregistrement"
                    };
                }
                catch
                {
                    return new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Erreur lors de l'enregistrement"
                    };
                }
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiResponse<string>>(
                jsonResponse,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return apiResponse ?? new ApiResponse<string> { Success = false, Message = "Erreur de sérialisation" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'enregistrement");
            return new ApiResponse<string>
            {
                Success = false,
                Message = "Une erreur est survenue lors de l'enregistrement"
            };
        }
    }
}
