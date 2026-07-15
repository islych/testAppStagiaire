using Microsoft.AspNetCore.Mvc;
using GestionDesStagiaires.Web.Models;
using GestionDesStagiaires.Web.Services;

namespace GestionDesStagiaires.Web.Controllers;

/// <summary>
/// Contrôleur pour fournir les données des formulaires de candidature
/// Cet endpoint fournit une interface simple pour le JavaScript du formulaire.
/// Les données proviennent entièrement de Candidatures.Service via MasterDataApiService.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CandidaturesFormController : ControllerBase
{
    private readonly IMasterDataApiService _masterDataApiService;
    private readonly ILogger<CandidaturesFormController> _logger;

    public CandidaturesFormController(
        IMasterDataApiService masterDataApiService,
        ILogger<CandidaturesFormController> logger)
    {
        _masterDataApiService = masterDataApiService;
        _logger = logger;
    }

    /// <summary>
    /// Récupère la liste des spécialités pour un département donné
    /// Cet endpoint délègue à Candidatures.Service
    /// </summary>
    /// <param name="departementId">ID du département</param>
    /// <returns>Liste des spécialités disponibles pour ce département</returns>
    /// <response code="200">Spécialités récupérées avec succès</response>
    /// <response code="400">Département invalide</response>
    [HttpGet("specialites/{departementId}")]
    [ProducesResponseType(typeof(List<SpecialiteOptionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSpecialitesByDepartement(int departementId)
    {
        try
        {
            _logger.LogInformation("Récupération des spécialités pour le département {DepartementId}", departementId);

            // Obtenir le token utilisateur si authentifié (optionnel pour les données publiques)
            var token = GetUserToken();

            // Appeler Candidatures.Service pour récupérer les spécialités
            var response = await _masterDataApiService.GetSpecialitesByDepartementAsync(departementId, token);

            if (!response.Success || response.Data == null)
            {
                _logger.LogWarning("Aucune spécialité trouvée pour le département {DepartementId}", departementId);
                return BadRequest(new ErrorResponse
                {
                    Error = "Département invalide",
                    Message = "Aucune spécialité disponible pour ce département"
                });
            }

            // Convertir en SpecialiteOptionResponse pour le frontend
            var specialites = response.Data.Select(s => new SpecialiteOptionResponse
            {
                Id = s.Id,
                Nom = s.Nom
            }).ToList();

            return Ok(specialites);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des spécialités");
            return BadRequest(new ErrorResponse
            {
                Error = "Erreur serveur",
                Message = ex.Message
            });
        }
    }

    /// <summary>
    /// Récupère la liste de tous les départements
    /// Cet endpoint délègue à Candidatures.Service
    /// </summary>
    /// <returns>Liste de tous les départements</returns>
    /// <response code="200">Départements récupérés avec succès</response>
    [HttpGet("departements")]
    [ProducesResponseType(typeof(List<DepartementOptionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDepartements()
    {
        try
        {
            _logger.LogInformation("Récupération de la liste des départements");

            // Obtenir le token utilisateur si authentifié (optionnel pour les données publiques)
            var token = GetUserToken();

            // Appeler Candidatures.Service pour récupérer tous les départements
            var response = await _masterDataApiService.GetDepartementsAsync(token);

            if (!response.Success || response.Data == null)
            {
                _logger.LogError("Erreur lors de la récupération des départements depuis Candidatures.Service");
                return StatusCode(500, new ErrorResponse
                {
                    Error = "Erreur serveur",
                    Message = "Impossible de récupérer les départements"
                });
            }

            // Convertir en DepartementOptionResponse pour le frontend
            var departements = response.Data.Select(d => new DepartementOptionResponse
            {
                Id = d.Id,
                Nom = d.Nom
            }).ToList();

            return Ok(departements);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des départements");
            return StatusCode(500, new ErrorResponse
            {
                Error = "Erreur serveur",
                Message = ex.Message
            });
        }
    }

    /// <summary>
    /// Récupère le token JWT depuis la session utilisateur (si disponible)
    /// </summary>
    private string GetUserToken()
    {
        // Si l'utilisateur est authentifié, récupérer le token depuis les claims
        var token = User?.FindFirst("access_token")?.Value ?? string.Empty;
        
        // Sinon, utiliser un token vide (pour les appels anonymes)
        return token;
    }
}

/// <summary>
/// Modèle de réponse d'erreur
/// </summary>
public class ErrorResponse
{
    public string Error { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
