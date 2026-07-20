using GestionDesStagiaires.Web.Models;
using GestionDesStagiaires.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GestionDesStagiaires.Web.Pages.RH;

[Authorize(Roles = "RH,Administrateur")]
public class IndexModel : PageModel
{
    private readonly ICandidaturesApiService _candidaturesService;
    private readonly IAuthenticationApiService _authService;
    private readonly IConfiguration _config;
    private readonly ILogger<IndexModel> _logger;

    public List<CandidatureViewModel> Candidatures { get; set; } = new();
    public Dictionary<int, string> NomsStagiaires { get; set; } = new();
    public string DocumentsApiUrl { get; set; } = string.Empty;
    public string CandidaturesApiUrl { get; set; } = string.Empty;
    public string JwtToken { get; set; } = string.Empty;
    public string? SuccessMessage { get; set; }
    public string? ErrorMessage { get; set; }

    [BindProperty]
    public string? CommentaireRefus { get; set; }

    public IndexModel(ICandidaturesApiService candidaturesService,
        IAuthenticationApiService authService,
        IConfiguration config, ILogger<IndexModel> logger)
    {
        _candidaturesService = candidaturesService;
        _authService = authService;
        _config = config;
        _logger = logger;
    }

    public async Task OnGetAsync()
    {
        DocumentsApiUrl = _config["ApiUrls:DocumentsApi"] ?? "http://localhost:5011";
        CandidaturesApiUrl = _config["ApiUrls:CandidaturesApi"] ?? "http://localhost:5296";
        JwtToken = HttpContext.Session.GetString("JwtToken") ?? string.Empty;
        SuccessMessage = TempData["SuccessMessage"]?.ToString();
        ErrorMessage = TempData["ErrorMessage"]?.ToString();

        if (string.IsNullOrEmpty(JwtToken)) return;

        try
        {
            var result = await _candidaturesService.GetAllCandidaturesAsync(JwtToken);
            if (!result.Success || result.Data == null) return;

            Candidatures = result.Data
                .Where(c => c.DestinataireTransmission == "RH" || c.DestinataireTransmission == "RH_Integre")
                .OrderByDescending(c => c.DateCreation)
                .ToList();

            foreach (var c in Candidatures.DistinctBy(x => x.StagiaireId))
            {
                var u = await _authService.GetUserByIdAsync(c.StagiaireId, JwtToken);
                if (u.Success && u.Data != null)
                    NomsStagiaires[c.StagiaireId] = $"{u.Data.Prenom} {u.Data.Nom}";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur chargement page RH");
            ErrorMessage = "Erreur lors du chargement.";
        }
    }

    public async Task<IActionResult> OnPostIntegrerAsync(Guid id)
    {
        var token = HttpContext.Session.GetString("JwtToken") ?? "";
        var result = await _candidaturesService.IntegrerStagiaireAsync(id, token);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
            result.Success ? "Stagiaire intégré dans le système." : result.Message;
        return RedirectToPage();
    }
    public async Task<IActionResult> OnPostRefuserAsync(Guid id)
    {
        var token = HttpContext.Session.GetString("JwtToken") ?? "";
        if (string.IsNullOrWhiteSpace(CommentaireRefus))
        {
            TempData["ErrorMessage"] = "Un motif est requis.";
            return RedirectToPage();
        }
        var result = await _candidaturesService.RefuserParDirectionAsync(id, CommentaireRefus, token);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
            result.Success ? "Candidature refusée." : result.Message;
        return RedirectToPage();
    }
}
