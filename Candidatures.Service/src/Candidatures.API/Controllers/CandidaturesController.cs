using Candidatures.Application.DTOs;
using Candidatures.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Candidatures.API.Controllers;

/// <summary>
/// Contrôleur pour la gestion des candidatures
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class CandidaturesController : ControllerBase
{
    private readonly ICandidatureService _service;
    private readonly ILogger<CandidaturesController> _logger;

    public CandidaturesController(
        ICandidatureService service,
        ILogger<CandidaturesController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Dépose une nouvelle candidature (Stagiaire uniquement)
    /// </summary>
    /// <param name="dto">Données de la candidature</param>
    /// <returns>Candidature créée</returns>
    /// <response code="201">Candidature créée avec succès</response>
    /// <response code="400">Données invalides</response>
    /// <response code="401">Non authentifié</response>
    [HttpPost]
    [Authorize(Roles = "Stagiaire")]
    [ProducesResponseType(typeof(ApiResponse<CandidatureDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCandidature([FromBody] CreateCandidatureDto dto)
    {
        try
        {
            _logger.LogInformation("Création d'une candidature pour le stagiaire {StagiaireId}", dto.StagiaireId);

            var candidature = await _service.CreateCandidatureAsync(dto);

            return CreatedAtAction(nameof(GetCandidatureById), new { id = candidature.Id },
                new ApiResponse<CandidatureDto>
                {
                    Success = true,
                    Message = "Candidature créée avec succès",
                    Data = candidature
                });
        }
        catch (ApplicationException ex)
        {
            _logger.LogWarning("Erreur lors de la création de candidature: {Message}", ex.Message);
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur serveur lors de la création de candidature");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Erreur serveur"
            });
        }
    }

    /// <summary>
    /// Récupère toutes les candidatures (Encadrant/Direction)
    /// </summary>
    /// <returns>Liste de toutes les candidatures</returns>
    /// <response code="200">Candidatures récupérées</response>
    /// <response code="401">Non authentifié</response>
    /// <response code="403">Non autorisé</response>
    [HttpGet]
    [Authorize(Roles = "Encadrant,Direction")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CandidatureDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllCandidatures()
    {
        try
        {
            _logger.LogInformation("Récupération de toutes les candidatures");

            var candidatures = await _service.GetAllCandidaturesAsync();

            return Ok(new ApiResponse<IEnumerable<CandidatureDto>>
            {
                Success = true,
                Message = $"{candidatures.Count()} candidature(s) trouvée(s)",
                Data = candidatures
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur serveur lors de la récupération des candidatures");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Erreur serveur"
            });
        }
    }

    /// <summary>
    /// Récupère une candidature par son ID
    /// </summary>
    /// <param name="id">ID de la candidature</param>
    /// <returns>Candidature trouvée</returns>
    /// <response code="200">Candidature trouvée</response>
    /// <response code="404">Candidature non trouvée</response>
    /// <response code="401">Non authentifié</response>
    [HttpGet("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<CandidatureDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCandidatureById(Guid id)
    {
        try
        {
            _logger.LogInformation("Récupération de la candidature {CandidatureId}", id);

            var candidature = await _service.GetCandidatureByIdAsync(id);

            if (candidature == null)
            {
                _logger.LogWarning("Candidature {CandidatureId} non trouvée", id);
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Candidature non trouvée"
                });
            }

            return Ok(new ApiResponse<CandidatureDto>
            {
                Success = true,
                Message = "Candidature trouvée",
                Data = candidature
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur serveur lors de la récupération de la candidature");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Erreur serveur"
            });
        }
    }

    /// <summary>
    /// Récupère les candidatures du stagiaire connecté
    /// </summary>
    /// <returns>Candidatures du stagiaire</returns>
    /// <response code="200">Candidatures récupérées</response>
    /// <response code="401">Non authentifié</response>
    [HttpGet("me/candidatures")]
    [Authorize(Roles = "Stagiaire")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CandidatureDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyCandidatures()
    {
        try
        {
            var stagiaireIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(stagiaireIdStr, out var stagiaireId))
            {
                _logger.LogWarning("Identifiant du stagiaire invalide");
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Identifiant du stagiaire invalide"
                });
            }

            _logger.LogInformation("Récupération des candidatures du stagiaire {StagiaireId}", stagiaireId);

            var candidatures = await _service.GetCandidaturesByStagiaireAsync(stagiaireId);

            return Ok(new ApiResponse<IEnumerable<CandidatureDto>>
            {
                Success = true,
                Message = $"{candidatures.Count()} candidature(s) trouvée(s)",
                Data = candidatures
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur serveur lors de la récupération des candidatures du stagiaire");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Erreur serveur"
            });
        }
    }

    /// <summary>
    /// Récupère le suivi d'une candidature (visible par le stagiaire)
    /// </summary>
    /// <param name="id">ID de la candidature</param>
    /// <returns>Suivi de la candidature</returns>
    /// <response code="200">Suivi récupéré</response>
    /// <response code="404">Candidature non trouvée</response>
    /// <response code="401">Non authentifié</response>
    [HttpGet("{id:guid}/suivi")]
    [Authorize(Roles = "Stagiaire")]
    [ProducesResponseType(typeof(ApiResponse<CandidatureSuiviDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCandidatureSuivi(Guid id)
    {
        try
        {
            _logger.LogInformation("Récupération du suivi de la candidature {CandidatureId}", id);

            var suivi = await _service.GetCandidatureSuiviAsync(id);

            if (suivi == null)
            {
                _logger.LogWarning("Candidature {CandidatureId} non trouvée", id);
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Candidature non trouvée"
                });
            }

            return Ok(new ApiResponse<CandidatureSuiviDto>
            {
                Success = true,
                Message = "Suivi récupéré",
                Data = suivi
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur serveur lors de la récupération du suivi");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Erreur serveur"
            });
        }
    }

    /// <summary>
    /// Accepte une candidature (Encadrant uniquement)
    /// </summary>
    /// <param name="id">ID de la candidature</param>
    /// <returns>Candidature acceptée</returns>
    /// <response code="200">Candidature acceptée</response>
    /// <response code="400">Candidature déjà traitée ou invalide</response>
    /// <response code="404">Candidature non trouvée</response>
    /// <response code="401">Non authentifié</response>
    /// <response code="403">Non autorisé</response>
    [HttpPost("{id:guid}/accept")]
    [Authorize(Roles = "Encadrant")]
    [ProducesResponseType(typeof(ApiResponse<CandidatureDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AcceptCandidature(Guid id)
    {
        try
        {
            var encadrantIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(encadrantIdStr, out var encadrantId))
            {
                _logger.LogWarning("Identifiant de l'encadrant invalide");
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Identifiant de l'encadrant invalide"
                });
            }

            _logger.LogInformation("Acceptation de la candidature {CandidatureId} par encadrant {EncadrantId}", id, encadrantId);

            // Vérifier que la candidature existe
            var exists = await _service.GetCandidatureByIdAsync(id);
            if (exists == null)
            {
                _logger.LogWarning("Candidature {CandidatureId} non trouvée", id);
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Candidature non trouvée"
                });
            }

            var candidature = await _service.AcceptCandidatureAsync(id, encadrantId);

            return Ok(new ApiResponse<CandidatureDto>
            {
                Success = true,
                Message = "Candidature acceptée avec succès",
                Data = candidature
            });
        }
        catch (ApplicationException ex)
        {
            _logger.LogWarning("Erreur métier lors de l'acceptation: {Message}", ex.Message);
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur serveur lors de l'acceptation");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Erreur serveur"
            });
        }
    }

    /// <summary>
    /// Refuse une candidature (Encadrant uniquement)
    /// </summary>
    /// <param name="id">ID de la candidature</param>
    /// <param name="dto">Données du refus (commentaire obligatoire)</param>
    /// <returns>Candidature refusée</returns>
    /// <response code="200">Candidature refusée</response>
    /// <response code="400">Candidature déjà traitée ou commentaire manquant</response>
    /// <response code="404">Candidature non trouvée</response>
    /// <response code="401">Non authentifié</response>
    /// <response code="403">Non autorisé</response>
    [HttpPost("{id:guid}/reject")]
    [Authorize(Roles = "Encadrant")]
    [ProducesResponseType(typeof(ApiResponse<CandidatureDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RejectCandidature(Guid id, [FromBody] RejectCandidatureRequestDto dto)
    {
        try
        {
            var encadrantIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(encadrantIdStr, out var encadrantId))
            {
                _logger.LogWarning("Identifiant de l'encadrant invalide");
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Identifiant de l'encadrant invalide"
                });
            }

            _logger.LogInformation("Refus de la candidature {CandidatureId} par encadrant {EncadrantId}", id, encadrantId);

            // Vérifier que la candidature existe
            var exists = await _service.GetCandidatureByIdAsync(id);
            if (exists == null)
            {
                _logger.LogWarning("Candidature {CandidatureId} non trouvée", id);
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Candidature non trouvée"
                });
            }

            var candidature = await _service.RejectCandidatureAsync(id, encadrantId, dto.Commentaire);

            return Ok(new ApiResponse<CandidatureDto>
            {
                Success = true,
                Message = "Candidature refusée avec succès",
                Data = candidature
            });
        }
        catch (ApplicationException ex)
        {
            _logger.LogWarning("Erreur métier lors du refus: {Message}", ex.Message);
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur serveur lors du refus");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Erreur serveur"
            });
        }
    }
}

/// <summary>
/// Réponse générique de l'API
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
}

/// <summary>
/// DTO pour le refus d'une candidature
/// </summary>
public class RejectCandidatureRequestDto
{
    /// <summary>
    /// Motif du refus (obligatoire)
    /// </summary>
    public string Commentaire { get; set; } = string.Empty;
}
