using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GestionDesStagiaires.Web.Models;
using GestionDesStagiaires.Web.Services;

namespace GestionDesStagiaires.Web.Pages.Candidatures;

[Authorize]
public class SuiviModel : PageModel
{
    private readonly ICandidaturesApiService _candidaturesService;
    private readonly ILogger<SuiviModel> _logger;

    public CandidatureSuiviViewModel? Suivi { get; set; }
    public string? ErrorMessage { get; set; }

    public SuiviModel(
        ICandidaturesApiService candidaturesService,
        ILogger<SuiviModel> logger)
    {
        _candidaturesService = candidaturesService;
        _logger = logger;
    }

    public async Task<IActionResult> OnGetAsync(Guid? id)
    {
        if (!id.HasValue)
        {
            return RedirectToPage("./Index");
        }

        try
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
            {
                ErrorMessage = "Session expirée";
                return Page();
            }

            var result = await _candidaturesService.GetCandidatureSuiviAsync(id.Value, token);

            if (result.Success && result.Data != null)
            {
                Suivi = result.Data;
            }
            else
            {
                ErrorMessage = "Candidature non trouvée";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération du suivi");
            ErrorMessage = "Une erreur est survenue";
        }

        return Page();
    }
}
