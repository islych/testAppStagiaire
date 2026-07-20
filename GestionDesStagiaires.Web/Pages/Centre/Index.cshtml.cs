using GestionDesStagiaires.Web.Models;
using GestionDesStagiaires.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Text.Json;

namespace GestionDesStagiaires.Web.Pages.Centre;

[Authorize(Roles = "Centre,Administrateur")]
public class IndexModel : PageModel
{
    private readonly ICandidaturesApiService _candidaturesService;
    private readonly IAuthenticationApiService _authService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _config;
    private readonly ILogger<IndexModel> _logger;

    public List<DossierCentre> DossiersCentre { get; set; } = new();
    public string DocumentsApiUrl { get; set; } = string.Empty;
    public string JwtToken { get; set; } = string.Empty;
    public string? SuccessMessage { get; set; }
    public string? ErrorMessage { get; set; }

    public IndexModel(
        ICandidaturesApiService candidaturesService,
        IAuthenticationApiService authService,
        IHttpClientFactory httpClientFactory,
        IConfiguration config,
        ILogger<IndexModel> logger)
    {
        _candidaturesService = candidaturesService;
        _authService = authService;
        _httpClientFactory = httpClientFactory;
        _config = config;
        _logger = logger;
    }

    public async Task OnGetAsync()
    {
        DocumentsApiUrl = _config["ApiUrls:DocumentsApi"] ?? "http://localhost:5011";
        JwtToken = HttpContext.Session.GetString("JwtToken") ?? string.Empty;
        SuccessMessage = TempData["SuccessMessage"]?.ToString();
        ErrorMessage = TempData["ErrorMessage"]?.ToString();

        if (string.IsNullOrEmpty(JwtToken)) return;

        try
        {
            // Récupérer les candidatures acceptées avec DestinataireTransmission == Centre
            var result = await _candidaturesService.GetAllCandidaturesAsync(JwtToken);
            if (!result.Success || result.Data == null) return;

            // Candidatures dont les documents ont été transmis au Centre par un encadrant
            var candidaturesAvecDocs = result.Data
                .Where(c => c.Statut == "Acceptee")
                .ToList();

            // Pour chaque candidature acceptée, vérifier si elle a des docs transmis au Centre
            foreach (var c in candidaturesAvecDocs)
            {
                var docsResult = await GetDocumentsByCandidatureAsync(c.Id, JwtToken);
                var hasDocsCentre = docsResult.Any(d =>
                    d.DestinataireActuel == "Centre" ||
                    d.Statut == "TransmisAuCentre" ||
                    d.Statut == "EnCorrectionSoumise" ||
                    d.Statut == "DemandeModification" ||
                    d.Statut == "Valide" ||
                    d.Statut == "Refuse");

                if (!hasDocsCentre) continue;

                var u = await _authService.GetUserByIdAsync(c.StagiaireId, JwtToken);
                DossiersCentre.Add(new DossierCentre
                {
                    CandidatureId = c.Id,
                    StagiaireId = c.StagiaireId,
                    NomComplet = u.Success && u.Data != null ? $"{u.Data.Prenom} {u.Data.Nom}" : $"#{c.StagiaireId}",
                    Email = u.Success && u.Data != null ? u.Data.Email : "",
                    DepartementNom = c.DepartementNom,
                    SpecialiteNom = c.SpecialiteNom
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur chargement page Centre");
            ErrorMessage = "Erreur lors du chargement.";
        }
    }

    private async Task<List<DocumentItem>> GetDocumentsByCandidatureAsync(Guid candidatureId, string token)
    {
        try
        {
            var docsApiUrl = _config["ApiUrls:DocumentsApi"] ?? "http://localhost:5011";
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(docsApiUrl);
            var req = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/documents/candidature/{candidatureId}");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var res = await client.SendAsync(req);
            if (!res.IsSuccessStatusCode) return new();
            var json = await res.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ApiResponseDocs>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return result?.Data ?? new();
        }
        catch { return new(); }
    }

    private class ApiResponseDocs
    {
        public bool Success { get; set; }
        public List<DocumentItem>? Data { get; set; }
    }

    private class DocumentItem
    {
        public string Statut { get; set; } = string.Empty;
        public string DestinataireActuel { get; set; } = "Encadrant";
    }
}

public class DossierCentre
{
    public Guid CandidatureId { get; set; }
    public int StagiaireId { get; set; }
    public string NomComplet { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DepartementNom { get; set; } = string.Empty;
    public string SpecialiteNom { get; set; } = string.Empty;
}
