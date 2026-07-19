using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;

namespace GestionDesStagiaires.Web.Pages.Documents;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<IndexModel> _logger;

    /// <summary>URL du Documents.Service — injectée dans la page pour les appels JS fetch</summary>
    public string DocumentsApiUrl { get; set; } = string.Empty;

    /// <summary>Token JWT de la session — passé à la page pour les appels JS fetch</summary>
    public string JwtToken { get; set; } = string.Empty;

    /// <summary>True si le stagiaire a au moins une candidature acceptée</summary>
    public bool ACandidatureAcceptee { get; set; } = false;

    /// <summary>Message d'erreur à afficher en cas d'impossibilité de vérifier</summary>
    public string? ErreurVerification { get; set; }

    public IndexModel(
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        ILogger<IndexModel> logger)
    {
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task OnGetAsync()
    {
        DocumentsApiUrl = _configuration["ApiUrls:DocumentsApi"] ?? "http://localhost:5011";
        JwtToken = HttpContext.Session.GetString("JwtToken") ?? string.Empty;

        _logger.LogInformation("Accès à la page Documents par {User}", User.Identity?.Name);

        // Seul le rôle Stagiaire a besoin de cette vérification
        if (!User.IsInRole("Stagiaire"))
        {
            ACandidatureAcceptee = true; // Encadrant/Admin peuvent toujours accéder
            return;
        }

        // Vérifier si le stagiaire a une candidature acceptée
        await VerifierCandidatureAccepteeAsync();
    }

    private async Task VerifierCandidatureAccepteeAsync()
    {
        try
        {
            var candidaturesApiUrl = _configuration["ApiUrls:CandidaturesApi"]
                                     ?? "http://localhost:5296";

            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(candidaturesApiUrl);
            client.Timeout = TimeSpan.FromSeconds(10);

            var request = new HttpRequestMessage(
                HttpMethod.Get, "/api/v1/candidatures/me/candidatures");
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", JwtToken);

            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Candidatures.Service a retourné {Status} pour {User}",
                    response.StatusCode, User.Identity?.Name);
                ACandidatureAcceptee = false;
                return;
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<CandidaturesApiResponse>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            ACandidatureAcceptee = result?.Data?.Any(c =>
                string.Equals(c.Statut, "Acceptee", StringComparison.OrdinalIgnoreCase))
                ?? false;

            _logger.LogInformation(
                "Stagiaire {User} — candidature acceptée : {Resultat}",
                User.Identity?.Name, ACandidatureAcceptee);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la vérification des candidatures");
            ACandidatureAcceptee = false;
            ErreurVerification =
                "Impossible de vérifier le statut de votre candidature. " +
                "Assurez-vous que le service des candidatures est démarré.";
        }
    }

    // DTOs internes pour désérialiser la réponse de Candidatures.Service
    private class CandidaturesApiResponse
    {
        public bool Success { get; set; }
        public List<CandidatureItem>? Data { get; set; }
    }

    private class CandidatureItem
    {
        public string Statut { get; set; } = string.Empty;
    }
}
