using Documents.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace Documents.Infrastructure.ExternalServices;

/// <summary>
/// Client HTTP vers le Notifications.Service
/// </summary>
public class NotificationServiceClient : INotificationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<NotificationServiceClient> _logger;

    public NotificationServiceClient(HttpClient httpClient, ILogger<NotificationServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task NotifyDocumentValideAsync(Guid documentId, int stagiaireId)
    {
        _logger.LogInformation("[NOTIF] Document {Id} validé pour stagiaire {SId}", documentId, stagiaireId);
        await Task.CompletedTask; // in-app notif peut être ajoutée si besoin
    }

    public async Task NotifyDocumentRefuseAsync(Guid documentId, int stagiaireId, string commentaire)
    {
        // Appelé quand un document est refusé — on notifie via Notifications.Service
        await PostAsync("/api/v1/notifications/demande-correction", new
        {
            stagiaireId,
            documentId,
            typeDocument = "Document",
            commentaire
        });
    }

    public async Task NotifyNouveauDocumentAsync(Guid documentId, int stagiaireId)
    {
        _logger.LogInformation("[NOTIF] Nouveau document {Id} par stagiaire {SId}", documentId, stagiaireId);
        await Task.CompletedTask;
    }

    public async Task NotifyDossierTransmisAuCentreAsync(Guid documentId, int stagiaireId)
    {
        _logger.LogInformation("[NOTIF] Dossier transmis au Centre — doc {Id} stagiaire {SId}", documentId, stagiaireId);
        await Task.CompletedTask;
    }

    public async Task NotifyModificationDemandeeAsync(Guid documentId, int stagiaireId, string commentaire)
    {
        await PostAsync("/api/v1/notifications/demande-modification", new
        {
            stagiaireId,
            documentId,
            commentaire
        });
    }

    public async Task NotifyCorrectionSoumiseCentreAsync(Guid documentId, int stagiaireId)
    {
        _logger.LogInformation("[NOTIF] Correction soumise au Centre — doc {Id} stagiaire {SId}", documentId, stagiaireId);
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
                _logger.LogWarning("Notifications.Service retourné {Status} pour {Path}", response.StatusCode, path);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Impossible de contacter Notifications.Service");
        }
    }
}
