using GestionDesStagiaires.Web.Models;
using GestionDesStagiaires.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GestionDesStagiaires.Web.Pages.Direction;

[Authorize(Roles = "Direction,Administrateur")]
public class IndexModel : PageModel
{
    private readonly ICandidaturesApiService _candidaturesService;
    private readonly IAuthenticationApiService _authService;
    private readonly ILogger<IndexModel> _logger;

    public List<CandidatureViewModel> Candidatures { get; set; } = new();
    public Dictionary<int, string> NomsStagiaires { get; set; } = new();
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    [BindProperty]
    public string? CommentaireRefus { get; set; }

    public IndexModel(
        ICandidaturesApiService candidaturesService,
        IAuthenticationApiService authService,
        ILogger<IndexModel> logger)
    {
        _candidaturesService = candidaturesService;
        _authService = authService;
        _logger = logger;
    }

    public async Task OnGetAsync()
    {
        var token = HttpContext.Session.GetString("JwtToken");
        if (string.IsNullOrEmpty(token)) return;

        SuccessMessage = TempData["SuccessMessage"]?.ToString();
        ErrorMessage = TempData["ErrorMessage"]?.ToString();

        try
        {
            var result = await _candidaturesService.GetAllCandidaturesAsync(token);
            if (!result.Success || result.Data == null) return;

            // Tous les statuts sauf EnAttente (direction voit transmises + traitées)
            Candidatures = result.Data
                .Where(c => c.Statut != "EnAttente")
                .OrderByDescending(c => c.DateCreation)
                .ToList();

            foreach (var c in Candidatures.DistinctBy(c => c.StagiaireId))
            {
                var u = await _authService.GetUserByIdAsync(c.StagiaireId, token);
                if (u.Success && u.Data != null)
                    NomsStagiaires[c.StagiaireId] = $"{u.Data.Prenom} {u.Data.Nom}";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur chargement page Direction");
            ErrorMessage = "Erreur lors du chargement.";
        }
    }

    public async Task<IActionResult> OnPostAccepterAsync(Guid id)
    {
        var token = HttpContext.Session.GetString("JwtToken") ?? "";
        var result = await _candidaturesService.AccepterParDirectionAsync(id, token);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
            result.Success ? "Accord de stage validé. Le stagiaire et l'encadrant ont été notifiés." : result.Message;
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRefuserAsync(Guid id)
    {
        var token = HttpContext.Session.GetString("JwtToken") ?? "";
        if (string.IsNullOrWhiteSpace(CommentaireRefus))
        {
            TempData["ErrorMessage"] = "Un motif de refus est requis.";
            return RedirectToPage();
        }
        var result = await _candidaturesService.RefuserParDirectionAsync(id, CommentaireRefus, token);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] =
            result.Success ? "Candidature refusée." : result.Message;
        return RedirectToPage();
    }
}
