using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GestionDesStagiaires.Web.Models;
using GestionDesStagiaires.Web.Services;
using System.Security.Claims;
using Candidatures.Domain.Enums;

namespace GestionDesStagiaires.Web.Pages.Candidatures;

[Authorize]
public class CreateModel : PageModel
{
    private readonly ICandidaturesApiService _candidaturesService;
    private readonly IMasterDataApiService _masterDataService;
    private readonly ILogger<CreateModel> _logger;

    // Validation des fichiers CV
    private readonly List<string> _allowedExtensions = new() { ".pdf", ".doc", ".docx" };
    private readonly List<string> _allowedMimeTypes = new() 
    { 
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
    };
    private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

    [BindProperty]
    public CreateCandidatureFormViewModel Candidature { get; set; } = new();

    public List<DepartementOptionResponse> Departements { get; set; } = new();
    public string? ErrorMessage { get; set; }

    public CreateModel(
        ICandidaturesApiService candidaturesService,
        IMasterDataApiService masterDataService,
        ILogger<CreateModel> logger)
    {
        _candidaturesService = candidaturesService;
        _masterDataService = masterDataService;
        _logger = logger;
    }

    /// <summary>
    /// Vérifie si un fichier a un format autorisé
    /// </summary>
    private bool IsValidCvFileFormat(IFormFile file)
    {
        if (file == null) return false;

        var extension = Path.GetExtension(file.FileName).ToLower();
        return _allowedExtensions.Contains(extension) || 
               _allowedMimeTypes.Contains(file.ContentType);
    }

    /// <summary>
    /// Vérifie si la taille du fichier est acceptable
    /// </summary>
    private bool IsValidCvFileSize(IFormFile file)
    {
        return file != null && file.Length <= MaxFileSize;
    }

    private string GetFileFormatErrorMessage()
    {
        return "Format de fichier non autorisé. Formats acceptés: PDF, DOC, DOCX";
    }

    private string GetFileSizeErrorMessage()
    {
        return "Le fichier est trop volumineux. Taille maximale: 5 MB";
    }

    public async Task OnGetAsync()
    {
        _logger.LogInformation("Page de création de candidature consultée");
        
        var token = HttpContext.Session.GetString("JwtToken");
        if (string.IsNullOrEmpty(token))
        {
            ErrorMessage = "Session expirée";
            return;
        }

        // Charger la liste des départements depuis l'API
        var departmentsResponse = await _masterDataService.GetDepartementsAsync(token);
        if (departmentsResponse.Success && departmentsResponse.Data != null)
        {
            Departements = departmentsResponse.Data.ToList();
        }
        else
        {
            _logger.LogWarning("Erreur lors de la récupération des départements");
            Departements = new();
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            var token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
            {
                ErrorMessage = "Session expirée";
                return Page();
            }

            // Recharger les départements pour le formulaire en cas d'erreur
            var departmentsResponse = await _masterDataService.GetDepartementsAsync(token);
            if (departmentsResponse.Success && departmentsResponse.Data != null)
            {
                Departements = departmentsResponse.Data.ToList();
            }

            // Valider le formulaire
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Valider le fichier CV
            if (Candidature.CvFile == null)
            {
                ModelState.AddModelError("Candidature.CvFile", "Veuillez sélectionner un fichier CV");
                return Page();
            }

            if (!IsValidCvFileFormat(Candidature.CvFile))
            {
                ModelState.AddModelError("Candidature.CvFile", GetFileFormatErrorMessage());
                return Page();
            }

            if (!IsValidCvFileSize(Candidature.CvFile))
            {
                ModelState.AddModelError("Candidature.CvFile", GetFileSizeErrorMessage());
                return Page();
            }

            _logger.LogInformation("Création d'une candidature par {User}", User.Identity?.Name);

            // Sauvegarder le fichier CV
            var cvPath = await SaveCvFileAsync(Candidature.CvFile);

            // Extraire le StagiaireId du JWT (le User.Id du token)
            var stagiaireIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? 
                                  User.FindFirst("sub")?.Value;
            
            if (!int.TryParse(stagiaireIdClaim, out var stagiaireId))
            {
                _logger.LogWarning("Impossible d'extraire le StagiaireId du JWT");
                ErrorMessage = "Erreur d'authentification";
                return Page();
            }

            // Mapper les données du formulaire vers CreateCandidatureViewModel
            var candidatureData = new CreateCandidatureViewModel
            {
                StagiaireId = stagiaireId,
                DepartementId = Candidature.DepartementId.GetValueOrDefault(),
                SpecialiteId = Candidature.SpecialiteId.GetValueOrDefault(),
                DureeStageMois = Candidature.DureeStageMois.GetValueOrDefault(),
                DateDebut = Candidature.DateDebut.GetValueOrDefault(),
                DateFin = Candidature.DateFin.GetValueOrDefault(),
                NiveauEtude = Enum.Parse<NiveauEtude>(Candidature.NiveauEtude ?? "BacPlus3"),
                Ecole = Candidature.Ecole ?? string.Empty,
                LettreMotivation = Candidature.LettreMotivation ?? string.Empty,
                CvFileName = Candidature.CvFile.FileName,
                CvPath = cvPath
            };

            var result = await _candidaturesService.CreateCandidatureAsync(candidatureData, token);

            if (result.Success)
            {
                TempData["StatusMessage"] = "Candidature créée avec succès";
                return RedirectToPage("./Index");
            }

            ErrorMessage = result.Message ?? "Erreur lors de la création";
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la création de candidature");
            ErrorMessage = "Une erreur est survenue";
            return Page();
        }
    }

    private async Task<string> SaveCvFileAsync(IFormFile file)
    {
        // Créer le dossier s'il n'existe pas
        var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "cvs");
        Directory.CreateDirectory(uploadPath);

        // Générer un nom de fichier sécurisé
        var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
        var filePath = Path.Combine(uploadPath, fileName);

        // Sauvegarder le fichier
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return $"/uploads/cvs/{fileName}";
    }
}
