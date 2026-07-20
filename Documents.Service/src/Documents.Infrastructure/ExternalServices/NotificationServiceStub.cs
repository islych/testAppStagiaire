using Documents.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Documents.Infrastructure.ExternalServices;

/// <summary>
/// Stub du service de notifications — à remplacer par l'appel réel à Notifications.Service
/// </summary>
public class NotificationServiceStub : INotificationService
{
    private readonly ILogger<NotificationServiceStub> _logger;

    public NotificationServiceStub(ILogger<NotificationServiceStub> logger)
    {
        _logger = logger;
    }

    public Task NotifyDocumentValideAsync(Guid documentId, int stagiaireId)
    {
        _logger.LogInformation(
            "[STUB] Notification : document {DocumentId} validé — stagiaire {StagiaireId}",
            documentId, stagiaireId);
        return Task.CompletedTask;
    }

    public Task NotifyDocumentRefuseAsync(Guid documentId, int stagiaireId, string commentaire)
    {
        _logger.LogInformation(
            "[STUB] Notification : document {DocumentId} refusé — stagiaire {StagiaireId} — motif : {Commentaire}",
            documentId, stagiaireId, commentaire);
        return Task.CompletedTask;
    }

    public Task NotifyNouveauDocumentAsync(Guid documentId, int stagiaireId)
    {
        _logger.LogInformation(
            "[STUB] Notification : nouveau document {DocumentId} déposé par le stagiaire {StagiaireId}",
            documentId, stagiaireId);
        return Task.CompletedTask;
    }

    public Task NotifyDossierTransmisAuCentreAsync(Guid documentId, int stagiaireId)
    {
        _logger.LogInformation(
            "[STUB] Notification : dossier transmis au Centre — document {DocumentId} — stagiaire {StagiaireId}",
            documentId, stagiaireId);
        return Task.CompletedTask;
    }

    public Task NotifyModificationDemandeeAsync(Guid documentId, int stagiaireId, string commentaire)
    {
        _logger.LogInformation(
            "[STUB] Notification : modification demandée sur {DocumentId} — stagiaire {StagiaireId} — commentaire : {Commentaire}",
            documentId, stagiaireId, commentaire);
        return Task.CompletedTask;
    }

    public Task NotifyCorrectionSoumiseCentreAsync(Guid documentId, int stagiaireId)
    {
        _logger.LogInformation(
            "[STUB] Notification : correction soumise au Centre — document {DocumentId} — stagiaire {StagiaireId}",
            documentId, stagiaireId);
        return Task.CompletedTask;
    }
}
