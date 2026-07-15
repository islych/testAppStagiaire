using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GestionDesStagiaires.Web.Models;
using GestionDesStagiaires.Web.Services;

namespace GestionDesStagiaires.Web.Pages.Candidatures;

[Authorize]
public class DetailsModel : PageModel
{
    private readonly ICandidaturesApiService _candidaturesService;
    private readonly ILogger<DetailsModel> _logger;

    public CandidatureViewModel? Candidature { get; set; }
    public string? ErrorMessage { get; set; }

    public DetailsModel(
        ICandidaturesApiService candidaturesService,
        ILogger<DetailsModel> logger)
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

            var result = await _candidaturesService.GetCandidatureByIdAsync(id.Value, token);

            if (result.Success && result.Data != null)
            {
                Candidature = result.Data;
            }
            else
            {
                ErrorMessage = "Candidature non trouvée";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des détails");
            ErrorMessage = "Une erreur est survenue";
        }

        return Page();
    }
}
