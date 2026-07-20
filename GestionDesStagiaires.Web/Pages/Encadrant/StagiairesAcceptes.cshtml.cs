using GestionDesStagiaires.Web.Models;
using GestionDesStagiaires.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GestionDesStagiaires.Web.Pages.Encadrant;

[Authorize(Roles = "Encadrant")]
public class StagiairesAcceptesModel : PageModel
{
    private readonly ICandidaturesApiService _candidaturesService;
    private readonly IAuthenticationApiService _authService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<StagiairesAcceptesModel> _logger;

    public List<StagiaireAvecDocs> Stagiaires { get; set; } = new();
    public string DocumentsApiUrl { get; set; } = string.Empty;
    public string CandidaturesApiUrl { get; set; } = string.Empty;
    public string JwtToken { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }

    public StagiairesAcceptesModel(
        ICandidaturesApiService candidaturesService,
        IAuthenticationApiService authService,
        IConfiguration configuration,
        ILogger<StagiairesAcceptesModel> logger)
    {
        _candidaturesService = candidaturesService;
        _authService = authService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task OnGetAsync()
    {
        DocumentsApiUrl = _configuration["ApiUrls:DocumentsApi"] ?? "http://localhost:5011";
        CandidaturesApiUrl = _configuration["ApiUrls:CandidaturesApi"] ?? "http://localhost:5296";
        JwtToken = HttpContext.Session.GetString("JwtToken") ?? string.Empty;

        if (string.IsNullOrEmpty(JwtToken)) return;

        try
        {
            var result = await _candidaturesService.GetAllCandidaturesAsync(JwtToken);
            if (!result.Success || result.Data == null) return;

            var acceptees = result.Data
                .Where(c => string.Equals(c.Statut, "Acceptee", StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var c in acceptees)
            {
                var info = new StagiaireAvecDocs
                {
                    CandidatureId = c.Id,
                    StagiaireId = c.StagiaireId,
                    DateAcceptation = c.DateDecision,
                    DejaTransmis = c.TransmisADirection,
                    DestinataireTransmission = c.DestinataireTransmission
                };

                var userResult = await _authService.GetUserByIdAsync(c.StagiaireId, JwtToken);
                if (userResult.Success && userResult.Data != null)
                {
                    info.NomComplet = $"{userResult.Data.Prenom} {userResult.Data.Nom}";
                    info.Email = userResult.Data.Email;
                }

                Stagiaires.Add(info);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur chargement stagiaires acceptés");
            ErrorMessage = "Impossible de charger la liste.";
        }
    }
}

public class StagiaireAvecDocs
{
    public Guid CandidatureId { get; set; }
    public int StagiaireId { get; set; }
    public string NomComplet { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime? DateAcceptation { get; set; }
    public bool DejaTransmis { get; set; }
    public string? DestinataireTransmission { get; set; }
}
