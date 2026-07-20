using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Notifications.Application.DTOs;
using Notifications.Application.Interfaces;
using System.Security.Claims;

namespace Notifications.API.Controllers;

[ApiController]
[Route("api/v1/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _service;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(INotificationService service, ILogger<NotificationsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>Déclenche une notification d'acceptation de candidature (appelé par Candidatures.Service)</summary>
    [HttpPost("candidature-acceptee")]
    [AllowAnonymous] // Appel service-to-service interne
    public async Task<IActionResult> CandidatureAcceptee([FromBody] CandidatureAccepteeRequest req)
    {
        await _service.EnvoyerNotificationCandidatureAccepteeAsync(
            req.StagiaireId, req.CandidatureId, req.EmailStagiaire, req.NomStagiaire);
        return Ok(new { success = true });
    }

    /// <summary>Déclenche une demande de correction (appelé par Documents.Service)</summary>
    [HttpPost("demande-correction")]
    [AllowAnonymous] // Appel service-to-service interne
    public async Task<IActionResult> DemandeCorrection([FromBody] DemandeCorrectionsRequest req)
    {
        await _service.EnvoyerDemandeCorrectionsDocumentAsync(
            req.StagiaireId, req.DocumentId, req.TypeDocument, req.Commentaire);
        return Ok(new { success = true });
    }

    /// <summary>Retourne les notifications du stagiaire connecté</summary>
    [HttpGet("me")]
    public async Task<ActionResult<IEnumerable<NotificationDto>>> GetMesNotifications()
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var notifs = await _service.GetNotificationsAsync(userId.Value);
        return Ok(new { success = true, data = notifs });
    }

    /// <summary>Retourne le nombre de notifications non lues</summary>
    [HttpGet("me/count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var count = await _service.GetUnreadCountAsync(userId.Value);
        return Ok(new { success = true, data = count });
    }

    /// <summary>Marque une notification comme lue</summary>
    [HttpPost("{id:guid}/lire")]
    public async Task<IActionResult> MarquerLue(Guid id)
    {
        await _service.MarquerCommeLueAsync(id);
        return Ok(new { success = true });
    }

    /// <summary>Marque toutes les notifications comme lues</summary>
    [HttpPost("me/tout-lire")]
    public async Task<IActionResult> MarquerToutLue()
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        await _service.MarquerToutesCommeLuesAsync(userId.Value);
        return Ok(new { success = true });
    }

    private int? GetUserId()
    {
        var str = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(str, out var id) ? id : null;
    }
}

public record CandidatureAccepteeRequest(int StagiaireId, Guid CandidatureId, string EmailStagiaire, string NomStagiaire);
public record DemandeCorrectionsRequest(int StagiaireId, Guid DocumentId, string TypeDocument, string Commentaire);
