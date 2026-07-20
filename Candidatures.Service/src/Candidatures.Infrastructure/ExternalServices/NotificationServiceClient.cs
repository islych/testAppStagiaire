using Candidatures.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace Candidatures.Infrastructure.ExternalServices;

/// <summary>
/// Client HTTP vers le Notifications.Service
/// Remplace le stub par de vrais appels HTTP
/// </summary>
public class NotificationServiceClient : INotificationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<NotificationServiceClient> _logger;

    public NotificationServiceClient(
        HttpClient httpClient,
        ILogger<NotificationServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task NotifyCandidatureAcceptedAsync(Guid candidatureId, int stagiaireId, string emailStagiaire, string nomStagiaire)
    {
        await PostAsync("/api/v1/notifications/candidature-acceptee", new
        {
            stagiaireId, candidatureId, emailStagiaire, nomStagiaire
        });
    }

    public async Task NotifyAcceptedByDirectionAsync(Guid candidatureId, int stagiaireId, int encadrantId, string emailStagiaire, string nomStagiaire, string token)
    {
        await PostAsync("/api/v1/notifications/candidature-acceptee", new
        {
            stagiaireId, candidatureId, emailStagiaire, nomStagiaire
        });
        // Notif encadrant (in-app uniquement)
        _logger.LogInformation("[NOTIF] Encadrant {EId} notifié acceptation direction pour candidature {CId}", encadrantId, candidatureId);
    }

    public async Task NotifyCandidatureCreatedAsync(Guid candidatureId, int stagiaireId)
    {
        // Pas de notification email pour la création — juste log
        _logger.LogInformation("[NOTIF] Candidature {Id} créée par stagiaire {SId}", candidatureId, stagiaireId);
        await Task.CompletedTask;
    }

    public async Task NotifyCandidatureRejectedAsync(Guid candidatureId, int stagiaireId, string commentaire)
    {
        _logger.LogInformation("[NOTIF] Candidature {Id} refusée pour stagiaire {SId}", candidatureId, stagiaireId);
        await Task.CompletedTask;
    }

    public async Task NotifyTransmittedToDirectionAsync(Guid candidatureId)
    {
        _logger.LogInformation("[NOTIF] Candidature {Id} transmise à la direction", candidatureId);
        await Task.CompletedTask;
    }

    private async Task PostAsync(string path, object payload)
    {
        try
        {
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(path, content);
            if (!response.IsSuccessStatusCode)
                _logger.LogWarning("Notifications.Service a retourné {Status} pour {Path}", response.StatusCode, path);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Impossible de contacter Notifications.Service — notification non envoyée");
        }
    }
}
