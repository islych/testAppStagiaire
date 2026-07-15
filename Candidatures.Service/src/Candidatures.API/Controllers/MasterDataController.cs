using Candidatures.Application.Interfaces;
using Candidatures.API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Candidatures.API.Controllers;

/// <summary>
/// Contrôleur pour les données de référence (départements, spécialités)
/// Ces données sont gérées de façon centralisée dans Candidatures.Domain.Enums
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class MasterDataController : ControllerBase
{
    private readonly IMasterDataService _masterDataService;
    private readonly ILogger<MasterDataController> _logger;

    public MasterDataController(
        IMasterDataService masterDataService,
        ILogger<MasterDataController> logger)
    {
        _masterDataService = masterDataService;
        _logger = logger;
    }

    /// <summary>
    /// GET /api/v1/masterdata/departements
    /// Récupère tous les départements
    /// </summary>
    /// <returns>Liste des départements</returns>
    /// <response code="200">Départements récupérés avec succès</response>
    [HttpGet("departements")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<DepartementDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDepartements()
    {
        try
        {
            _logger.LogInformation("Récupération de tous les départements");

            var departements = await _masterDataService.GetDepartementsAsync();
            var specialitesAll = await _masterDataService.GetAllSpecialitesAsync();

            var data = departements.Select(d => new DepartementDto
            {
                Id = d.Id,
                Nom = d.Nom,
                Description = d.Description,
                NombreSpecialites = specialitesAll.Count(s => s.DepartementId == d.Id)
            });

            return Ok(new ApiResponse<IEnumerable<DepartementDto>>
            {
                Success = true,
                Message = $"{departements.Count()} département(s) trouvé(s)",
                Data = data
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur serveur lors de la récupération des départements");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Erreur serveur"
            });
        }
    }

    /// <summary>
    /// GET /api/v1/masterdata/departements/{departementId}
    /// Récupère le détail d'un département avec ses spécialités
    /// </summary>
    /// <param name="departementId">ID du département</param>
    /// <returns>Détail du département</returns>
    /// <response code="200">Département trouvé</response>
    /// <response code="404">Département non trouvé</response>
    [HttpGet("departements/{departementId}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<DepartementDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDepartementById(int departementId)
    {
        try
        {
            _logger.LogInformation("Récupération du département {DepartementId}", departementId);

            var departement = await _masterDataService.GetDepartementByIdAsync(departementId);
            if (departement == null)
            {
                _logger.LogWarning("Département {DepartementId} non trouvé", departementId);
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Département non trouvé"
                });
            }

            var specialites = await _masterDataService.GetSpecialitesByDepartementAsync(departementId);
            var data = new DepartementDetailDto
            {
                Id = departement.Id,
                Nom = departement.Nom,
                Description = departement.Description,
                Specialites = specialites.Select(s => new SpecialiteDto
                {
                    Id = s.Id,
                    Nom = s.Nom,
                    DepartementId = s.DepartementId
                }).ToList()
            };

            return Ok(new ApiResponse<DepartementDetailDto>
            {
                Success = true,
                Message = "Département trouvé",
                Data = data
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur serveur lors de la récupération du département");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Erreur serveur"
            });
        }
    }

    /// <summary>
    /// GET /api/v1/masterdata/departements/{departementId}/specialites
    /// Récupère les spécialités d'un département
    /// </summary>
    /// <param name="departementId">ID du département</param>
    /// <returns>Liste des spécialités du département</returns>
    /// <response code="200">Spécialités récupérées avec succès</response>
    /// <response code="404">Département non trouvé</response>
    [HttpGet("departements/{departementId}/specialites")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<SpecialiteDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSpecialitesByDepartement(int departementId)
    {
        try
        {
            _logger.LogInformation("Récupération des spécialités du département {DepartementId}", departementId);

            // Vérifier que le département existe
            var departement = await _masterDataService.GetDepartementByIdAsync(departementId);
            if (departement == null)
            {
                _logger.LogWarning("Département {DepartementId} non trouvé", departementId);
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Département non trouvé"
                });
            }

            var specialites = await _masterDataService.GetSpecialitesByDepartementAsync(departementId);
            var data = specialites.Select(s => new SpecialiteDto
            {
                Id = s.Id,
                Nom = s.Nom,
                DepartementId = s.DepartementId
            });

            return Ok(new ApiResponse<IEnumerable<SpecialiteDto>>
            {
                Success = true,
                Message = $"{specialites.Count()} spécialité(s) trouvée(s)",
                Data = data
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur serveur lors de la récupération des spécialités");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Erreur serveur"
            });
        }
    }

    /// <summary>
    /// GET /api/v1/masterdata/specialites/{departementId}
    /// Alias court pour obtenir les spécialités d'un département
    /// </summary>
    /// <param name="departementId">ID du département</param>
    /// <returns>Liste des spécialités du département</returns>
    [HttpGet("specialites/{departementId}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<SpecialiteDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSpecialites(int departementId)
    {
        return await GetSpecialitesByDepartement(departementId);
    }

    /// <summary>
    /// GET /api/v1/masterdata/specialites
    /// Récupère toutes les spécialités
    /// </summary>
    /// <returns>Liste de toutes les spécialités</returns>
    /// <response code="200">Spécialités récupérées avec succès</response>
    [HttpGet("specialites")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<SpecialiteDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllSpecialites()
    {
        try
        {
            _logger.LogInformation("Récupération de toutes les spécialités");

            var specialites = await _masterDataService.GetAllSpecialitesAsync();
            var data = specialites.Select(s => new SpecialiteDto
            {
                Id = s.Id,
                Nom = s.Nom,
                DepartementId = s.DepartementId
            });

            return Ok(new ApiResponse<IEnumerable<SpecialiteDto>>
            {
                Success = true,
                Message = $"{specialites.Count()} spécialité(s) trouvée(s)",
                Data = data
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur serveur lors de la récupération des spécialités");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Erreur serveur"
            });
        }
    }
}
