using GestionDesStagiaires.Web.Models;
using GestionDesStagiaires.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GestionDesStagiaires.Web.Pages.Centre;

namespace GestionDesStagiaires.Web.Pages.RH;

[Authorize(Roles = "RH,Administrateur")]
public class DossiersModel : PageModel
{
    private readonly ICandidaturesApiService _candidaturesService;
    private readonly IAuthenticationApiService _authService;
    private readonly IConfiguration _config;

    public List<DossierStagiaire> Dossiers { get; set; } = new();
    public string DocumentsApiUrl { get; set; } = string.Empty;
    public string JwtToken { get; set; } = string.Empty;

    public DossiersModel(ICandidaturesApiService c, IAuthenticationApiService a, IConfiguration cfg)
    {
        _candidaturesService = c; _authService = a; _config = cfg;
    }

    public async Task OnGetAsync()
    {
        DocumentsApiUrl = _config["ApiUrls:DocumentsApi"] ?? "http://localhost:5011";
        JwtToken = HttpContext.Session.GetString("JwtToken") ?? string.Empty;
        if (string.IsNullOrEmpty(JwtToken)) return;

        var result = await _candidaturesService.GetAllCandidaturesAsync(JwtToken);
        if (!result.Success || result.Data == null) return;

        var acceptees = result.Data
            .Where(c => c.Statut == "Acceptee" &&
                        string.Equals(c.DestinataireTransmission, "RH", StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var c in acceptees)
        {
            var u = await _authService.GetUserByIdAsync(c.StagiaireId, JwtToken);
            Dossiers.Add(new DossierStagiaire
            {
                CandidatureId = c.Id,
                StagiaireId = c.StagiaireId,
                NomComplet = u.Success && u.Data != null ? $"{u.Data.Prenom} {u.Data.Nom}" : $"#{c.StagiaireId}",
                Email = u.Success && u.Data != null ? u.Data.Email : "",
                DepartementNom = c.DepartementNom,
                SpecialiteNom = c.SpecialiteNom,
                DateAcceptation = c.DateDecision
            });
        }
    }
}
