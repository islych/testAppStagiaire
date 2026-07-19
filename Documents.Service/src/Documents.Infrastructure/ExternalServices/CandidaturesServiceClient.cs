using Documents.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Documents.Infrastructure.ExternalServices;

/// <summary>
/// Client HTTP vers Candidatures.Service.
/// Interroge l'endpoint /api/v1/candidatures/me/candidatures pour vérifier
/// si le stagiaire connecté a au moins une candidature avec le statut "Acceptee".
/// </summary>
public class CandidaturesServiceClient : ICandidaturesServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CandidaturesServiceClient> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public CandidaturesServiceClient(
        HttpClient httpClient,
        ILogger<CandidaturesServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Retourne true si le stagiaire a au moins une candidature avec statut "Acceptee".
    /// En cas d'erreur de communication, retourne false par sécurité (fail-closed).
    /// </summary>
    public async Task<bool> StagiaireACandidatureAccepteeAsync(int stagiaireId, string jwtToken)
    {
        try
        {
            var request = new HttpRequestMessage(
                HttpMethod.Get,
                "/api/v1/candidatures/me/candidatures");

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", jwtToken);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Candidatures.Service a retourné {StatusCode} pour le stagiaire {StagiaireId}",
                    response.StatusCode, stagiaireId);
                return false;
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<CandidaturesResponse>(json, JsonOptions);

            if (result?.Data == null)
                return false;

            // Vérifier qu'au moins une candidature est acceptée
            var aUneAcceptee = result.Data.Any(c =>
                string.Equals(c.Statut, "Acceptee", StringComparison.OrdinalIgnoreCase));

            _logger.LogInformation(
                "Stagiaire {StagiaireId} — candidature acceptée trouvée : {Resultat}",
                stagiaireId, aUneAcceptee);

            return aUneAcceptee;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Erreur lors de la vérification des candidatures du stagiaire {StagiaireId}",
                stagiaireId);
            // Fail-closed : en cas d'erreur réseau, on bloque l'upload par sécurité
            return false;
        }
    }

    // ─── DTOs internes pour désérialiser la réponse ───────────────────────────

    private class CandidaturesResponse
    {
        public bool Success { get; set; }
        public List<CandidatureItem>? Data { get; set; }
    }

    private class CandidatureItem
    {
        public string Statut { get; set; } = string.Empty;
    }
}
