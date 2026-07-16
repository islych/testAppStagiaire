using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GestionDesStagiaires.Web.Models;
using GestionDesStagiaires.Web.Services;
using System.Security.Claims;

namespace GestionDesStagiaires.Web.Pages.Encadrant;

[Authorize(Roles = "Encadrant")]
public class IndexModel : PageModel
{
    private readonly ICandidaturesApiService _candidaturesService;
    private readonly IAuthenticationApiService _authService;
    private readonly ILogger<IndexModel> _logger;

    [BindProperty(SupportsGet = true)]
    public string? FilterStatus { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    public List<CandidatureViewModel> Candidatures { get; set; } = new();
    public Dictionary<int, EncadrantUserInfo> StagiairesInfo { get; set; } = new();
    public string? ErrorMessage { get; set; }

    public IndexModel(
        ICandidaturesApiService candidaturesService,
        IAuthenticationApiService authService,
        ILogger<IndexModel> logger)
    {
        _candidaturesService = candidaturesService;
        _authService = authService;
        _logger = logger;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            _logger.LogInformation("Page gestion candidatures consultée par encadrant");

            var token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
            {
                ErrorMessage = "Session expirée";
                return Page();
            }

            // Récupérer toutes les candidatures
            var result = await _candidaturesService.GetAllCandidaturesAsync(token);

            if (!result.Success || result.Data == null)
            {
                _logger.LogWarning("Erreur récupération candidatures");
                ErrorMessage = "Erreur lors du chargement des candidatures";
                return Page();
            }

            Candidatures = result.Data.ToList();

            // Filtrer par statut si spécifié
            if (!string.IsNullOrEmpty(FilterStatus))
            {
                Candidatures = Candidatures.Where(c => c.Statut == FilterStatus).ToList();
            }

            // Récupérer les informations des stagiaires
            foreach (var candidature in Candidatures.DistinctBy(c => c.StagiaireId))
            {
                var userResult = await _authService.GetUserByIdAsync(candidature.StagiaireId, token);
                if (userResult.Success && userResult.Data != null)
                {
                    StagiairesInfo[candidature.StagiaireId] = new EncadrantUserInfo
                    {
                        Nom = userResult.Data.Nom,
                        Prenom = userResult.Data.Prenom,
                        Email = userResult.Data.Email
                    };
                }
            }

            // Rechercher par nom/prénom du stagiaire
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                var searchLower = SearchTerm.ToLower();
                Candidatures = Candidatures.Where(c =>
                {
                    var info = StagiairesInfo.ContainsKey(c.StagiaireId)
                        ? StagiairesInfo[c.StagiaireId]
                        : null;
                    var fullName = info?.FullName ?? "";
                    return fullName.ToLower().Contains(searchLower);
                }).ToList();
            }

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du chargement des candidatures");
            ErrorMessage = "Une erreur est survenue";
            return Page();
        }
    }
}

public class EncadrantUserInfo
{
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName => $"{Prenom} {Nom}";
}
