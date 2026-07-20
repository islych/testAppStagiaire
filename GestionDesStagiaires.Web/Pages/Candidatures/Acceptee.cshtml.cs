using GestionDesStagiaires.Web.Models;
using GestionDesStagiaires.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GestionDesStagiaires.Web.Pages.Candidatures;

[Authorize(Roles = "Stagiaire")]
public class AccepteeModel : PageModel
{
    private readonly ICandidaturesApiService _candidaturesService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AccepteeModel> _logger;

    public CandidatureViewModel? CandidatureAcceptee { get; set; }
    public string DocumentsApiUrl { get; set; } = string.Empty;
    public string JwtToken { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }

    public AccepteeModel(
        ICandidaturesApiService candidaturesService,
        IConfiguration configuration,
        ILogger<AccepteeModel> logger)
    {
        _candidaturesService = candidaturesService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task OnGetAsync()
    {
        DocumentsApiUrl = _configuration["ApiUrls:DocumentsApi"] ?? "http://localhost:5011";
        JwtToken = HttpContext.Session.GetString("JwtToken") ?? string.Empty;

        if (string.IsNullOrEmpty(JwtToken)) return;

        try
        {
            var result = await _candidaturesService.GetMyCandidaturesAsync(JwtToken);
            if (result.Success && result.Data != null)
            {
                CandidatureAcceptee = result.Data
                    .FirstOrDefault(c => string.Equals(c.Statut, "Acceptee", StringComparison.OrdinalIgnoreCase));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur récupération candidature acceptée");
            ErrorMessage = "Impossible de charger votre candidature.";
        }
    }
}
