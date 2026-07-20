using Candidatures.Application.DTOs;
using Candidatures.Application.Interfaces;
using Candidatures.Domain.Enums;
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
    [Authorize(Roles = "Encadrant,Direction,Centre,RH,Administrateur")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<CandidatureDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllCandidatures()
    {
        try
        {
            _logger.LogInformation("Récupération de toutes les candidatures");

            IEnumerable<CandidatureDto> candidatures;

            // Si c'est un encadrant, filtrer par son département
            var departementClaim = User.FindFirst("departementId")?.Value;
            if (User.IsInRole("Encadrant") && int.TryParse(departementClaim, out var departementId))
            {
                _logger.LogInformation("Filtrage par département {DeptId} pour l'encadrant", departementId);
                candidatures = await _service.GetCandidaturesByDepartementAsync(departementId);
            }
            else
            {
                candidatures = await _service.GetAllCandidaturesAsync();
            }

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

    /// <summary>Encadrant transmet la candidature à la Direction</summary>
    [HttpPost("{id:guid}/transmettre-direction")]
    [Authorize(Roles = "Encadrant")]
    public async Task<IActionResult> TransmettreADirection(Guid id)
    {
        try
        {
            var encadrantIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(encadrantIdStr, out var encadrantId))
                return Unauthorized(new ApiResponse<object> { Success = false, Message = "Identifiant invalide" });

            var candidature = await _service.TransmettreADirectionAsync(id, encadrantId);
            return Ok(new ApiResponse<CandidatureDto> { Success = true, Message = "Candidature transmise à la Direction.", Data = candidature });
        }
        catch (ApplicationException ex)
        {
            return BadRequest(new ApiResponse<object> { Success = false, Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur transmission direction {Id}", id);
            return StatusCode(500, new ApiResponse<object> { Success = false, Message = "Erreur serveur" });
        }
    }

    /// <summary>RH intègre le stagiaire dans le système</summary>
    [HttpPost("{id:guid}/integrer")]
    [Authorize(Roles = "RH,Administrateur")]
    public async Task<IActionResult> IntegrerStagiaire(Guid id)
    {
        try
        {
            var candidature = await _service.IntegrerStagiaireAsync(id);
            return Ok(new ApiResponse<CandidatureDto> { Success = true, Message = "Stagiaire intégré dans le système.", Data = candidature });
        }
        catch (ApplicationException ex)
        {
            return BadRequest(new ApiResponse<object> { Success = false, Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur intégration stagiaire {Id}", id);
            return StatusCode(500, new ApiResponse<object> { Success = false, Message = "Erreur serveur" });
        }
    }

    /// <summary>Centre transmet la candidature acceptée au RH</summary>
    [HttpPost("{id:guid}/transmettre-rh")]
    [Authorize(Roles = "Centre,Administrateur")]
    public async Task<IActionResult> TransmettreRH(Guid id)
    {
        try
        {
            var candidature = await _service.TransmettreRHAsync(id);
            return Ok(new ApiResponse<CandidatureDto> { Success = true, Message = "Candidature transmise au RH.", Data = candidature });
        }
        catch (ApplicationException ex)
        {
            return BadRequest(new ApiResponse<object> { Success = false, Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur transmission RH {Id}", id);
            return StatusCode(500, new ApiResponse<object> { Success = false, Message = "Erreur serveur" });
        }
    }

    /// <summary>Direction transmet la candidature au Centre</summary>
    [HttpPost("{id:guid}/transmettre-centre")]
    [Authorize(Roles = "Direction,Administrateur")]
    public async Task<IActionResult> TransmettreCentre(Guid id)
    {
        try
        {
            var candidature = await _service.TransmettreCentreAsync(id);
            return Ok(new ApiResponse<CandidatureDto> { Success = true, Message = "Candidature transmise au Centre.", Data = candidature });
        }
        catch (ApplicationException ex)
        {
            return BadRequest(new ApiResponse<object> { Success = false, Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur transmission centre {Id}", id);
            return StatusCode(500, new ApiResponse<object> { Success = false, Message = "Erreur serveur" });
        }
    }

    /// <summary>Encadrant refuse directement une candidature</summary>
    [HttpPost("{id:guid}/reject")]
    [Authorize(Roles = "Encadrant")]
    public async Task<IActionResult> RejectCandidature(Guid id, [FromBody] RejectCandidatureRequestDto dto)
    {
        try
        {
            var encadrantIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(encadrantIdStr, out var encadrantId))
                return Unauthorized(new ApiResponse<object> { Success = false, Message = "Identifiant invalide" });

            var candidature = await _service.RejectCandidatureAsync(id, encadrantId, dto.Commentaire);
            return Ok(new ApiResponse<CandidatureDto> { Success = true, Message = "Candidature refusée.", Data = candidature });
        }
        catch (ApplicationException ex)
        {
            return BadRequest(new ApiResponse<object> { Success = false, Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur refus {Id}", id);
            return StatusCode(500, new ApiResponse<object> { Success = false, Message = "Erreur serveur" });
        }
    }

    /// <summary>Direction accepte une candidature transmise → notifie stagiaire + encadrant</summary>
    [HttpPost("{id:guid}/accepter-direction")]
    [Authorize(Roles = "Direction,Centre,RH,Administrateur")]
    public async Task<IActionResult> AccepterParDirection(Guid id)
    {
        try
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "").Trim();
            var candidature = await _service.AccepterParDirectionAsync(id, token);
            return Ok(new ApiResponse<CandidatureDto> { Success = true, Message = "Candidature acceptée. Le stagiaire et l'encadrant ont été notifiés.", Data = candidature });
        }
        catch (ApplicationException ex)
        {
            return BadRequest(new ApiResponse<object> { Success = false, Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur acceptation direction {Id}", id);
            return StatusCode(500, new ApiResponse<object> { Success = false, Message = "Erreur serveur" });
        }
    }

    /// <summary>Direction refuse une candidature transmise</summary>
    [HttpPost("{id:guid}/refuser-direction")]
    [Authorize(Roles = "Direction,Centre,RH,Administrateur")]
    public async Task<IActionResult> RefuserParDirection(Guid id, [FromBody] RejectCandidatureRequestDto dto)
    {
        try
        {
            var candidature = await _service.RefuserParDirectionAsync(id, dto.Commentaire);
            return Ok(new ApiResponse<CandidatureDto> { Success = true, Message = "Candidature refusée par la Direction.", Data = candidature });
        }
        catch (ApplicationException ex)
        {
            return BadRequest(new ApiResponse<object> { Success = false, Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur refus direction {Id}", id);
            return StatusCode(500, new ApiResponse<object> { Success = false, Message = "Erreur serveur" });
        }
    }

    /// <summary>Récupère les candidatures transmises à la Direction</summary>
    [HttpGet("direction")]
    [Authorize(Roles = "Direction,Centre,RH,Administrateur")]
    public async Task<IActionResult> GetCandidaturesDirection()
    {
        try
        {
            var toutes = await _service.GetAllCandidaturesAsync();
            var transmises = toutes.Where(c => c.Statut == CandidatureStatus.TransmiseADirection);
            return Ok(new ApiResponse<IEnumerable<CandidatureDto>> { Success = true, Data = transmises });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur récupération candidatures direction");
            return StatusCode(500, new ApiResponse<object> { Success = false, Message = "Erreur serveur" });
        }
    }

    /// <summary>Ancien endpoint conservé pour compatibilité web</summary>
    [HttpPost("{id:guid}/accept")]
    [Authorize(Roles = "Encadrant")]
    public async Task<IActionResult> AcceptCandidature(Guid id)
        => await TransmettreADirection(id);

    /// <summary>Transmettre dossier (ancien endpoint)</summary>
    [HttpPost("{id:guid}/transmettre")]
    [Authorize(Roles = "Encadrant")]
    public async Task<IActionResult> TransmettreDossier(Guid id, [FromBody] TransmettreDto dto)
        => await TransmettreADirection(id);

    /// <summary>
    /// Appelé automatiquement par Documents.Service quand tous les documents d'une candidature sont validés.
    /// Marque le dossier comme "DossierAccepte" (DestinataireTransmission = "DossierAccepte").
    /// </summary>
    [HttpPost("{id:guid}/dossier-accepte")]
    [Authorize(Roles = "Centre,Administrateur")]
    public async Task<IActionResult> MarquerDossierAccepte(Guid id)
    {
        try
        {
            _logger.LogInformation("Marquage automatique du dossier {CandidatureId} comme accepté", id);
            var candidature = await _service.MarquerDossierAccepteAsync(id);
            return Ok(new ApiResponse<CandidatureDto> { Success = true, Message = "Dossier marqué comme accepté.", Data = candidature });
        }
        catch (ApplicationException ex)
        {
            return BadRequest(new ApiResponse<object> { Success = false, Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur marquage dossier accepté {Id}", id);
            return StatusCode(500, new ApiResponse<object> { Success = false, Message = "Erreur serveur" });
        }
    }
}

public record TransmettreDto(string Destinataire);
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
