using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GestionDesStagiaires.Web.Models;
using GestionDesStagiaires.Web.Services;

namespace GestionDesStagiaires.Web.Pages.Candidatures;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ICandidaturesApiService _candidaturesService;
    private readonly ILogger<IndexModel> _logger;

    public IEnumerable<CandidatureViewModel> Candidatures { get; set; } = new List<CandidatureViewModel>();
    public string? ErrorMessage { get; set; }

    public IndexModel(
        ICandidaturesApiService candidaturesService,
        ILogger<IndexModel> logger)
    {
        _candidaturesService = candidaturesService;
        _logger = logger;
    }

    public async Task OnGetAsync()
    {
        try
        {
            _logger.LogInformation("Accès à la liste des candidatures par {User}", User.Identity?.Name);

            // Récupérer le token JWT de la session
            var token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
            {
                ErrorMessage = "Session expirée";
                return;
            }

            // Appel au service pour récupérer les candidatures
            var result = await _candidaturesService.GetMyCandidaturesAsync(token);

            if (result.Success && result.Data != null)
            {
                Candidatures = result.Data;
            }
            else
            {
                ErrorMessage = result.Message ?? "Erreur lors de la récupération des candidatures";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des candidatures");
            ErrorMessage = "Une erreur est survenue";
        }
    }
}
