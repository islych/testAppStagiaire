using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GestionDesStagiaires.Web.Models;
using GestionDesStagiaires.Web.Services;
using System.Security.Claims;

namespace GestionDesStagiaires.Web.Pages.Encadrant;

[Authorize(Roles = "Encadrant")]
public class DetailsModel : PageModel
{
    private readonly ICandidaturesApiService _candidaturesService;
    private readonly IAuthenticationApiService _authService;
    private readonly ILogger<DetailsModel> _logger;

    [BindProperty]
    public string? CommentaireRefus { get; set; }

    public CandidatureViewModel? Candidature { get; set; }
    public EncadrantDetailsUserInfo? StagiaireInfo { get; set; }
    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    public DetailsModel(
        ICandidaturesApiService candidaturesService,
        IAuthenticationApiService authService,
        ILogger<DetailsModel> logger)
    {
        _candidaturesService = candidaturesService;
        _authService = authService;
        _logger = logger;
    }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Détails candidature {CandidatureId} consultés par encadrant", id);

            var token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
            {
                ErrorMessage = "Session expirée";
                return Page();
            }

            // Récupérer la candidature
            var result = await _candidaturesService.GetCandidatureByIdAsync(id, token);
            if (!result.Success || result.Data == null)
            {
                ErrorMessage = "Candidature non trouvée";
                return Page();
            }

            Candidature = result.Data;

            // Récupérer les informations du stagiaire
            var userResult = await _authService.GetUserByIdAsync(Candidature.StagiaireId, token);
            if (userResult.Success && userResult.Data != null)
            {
                StagiaireInfo = new EncadrantDetailsUserInfo
                {
                    Nom = userResult.Data.Nom,
                    Prenom = userResult.Data.Prenom,
                    Email = userResult.Data.Email
                };
            }

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des détails de candidature");
            ErrorMessage = "Une erreur est survenue";
            return Page();
        }
    }

    public async Task<IActionResult> OnPostAcceptAsync(Guid id)
    {
        try
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "Session expirée";
                return RedirectToPage("/Account/Login");
            }

            _logger.LogInformation("Acceptation candidature {CandidatureId} par encadrant", id);

            var result = await _candidaturesService.AcceptCandidatureAsync(id, token);

            if (result.Success)
            {
                TempData["SuccessMessage"] = "Candidature acceptée avec succès";
                return RedirectToPage("/Encadrant/Index");
            }

            TempData["ErrorMessage"] = result.Message ?? "Erreur lors de l'acceptation";
            return RedirectToPage("/Encadrant/Details", new { id = id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'acceptation de candidature");
            TempData["ErrorMessage"] = "Une erreur est survenue";
            return RedirectToPage("/Encadrant/Details", new { id = id });
        }
    }

    public async Task<IActionResult> OnPostRejectAsync(Guid id)
    {
        try
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "Session expirée";
                return RedirectToPage("/Account/Login");
            }

            if (string.IsNullOrEmpty(CommentaireRefus))
            {
                TempData["ErrorMessage"] = "Veuillez saisir un motif de refus";
                return RedirectToPage("/Encadrant/Details", new { id = id });
            }

            _logger.LogInformation("Refus candidature {CandidatureId} par encadrant", id);

            var result = await _candidaturesService.RejectCandidatureAsync(id, CommentaireRefus, token);

            if (result.Success)
            {
                TempData["SuccessMessage"] = "Candidature refusée avec succès";
                return RedirectToPage("/Encadrant/Index");
            }

            TempData["ErrorMessage"] = result.Message ?? "Erreur lors du refus";
            return RedirectToPage("/Encadrant/Details", new { id = id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du refus de candidature");
            TempData["ErrorMessage"] = "Une erreur est survenue";
            return RedirectToPage("/Encadrant/Details", new { id = id });
        }
    }
}

public class EncadrantDetailsUserInfo
{
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName => $"{Prenom} {Nom}";
}
