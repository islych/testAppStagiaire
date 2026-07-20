using Candidatures.Application.Interfaces;

namespace Candidatures.Infrastructure.ExternalServices;

/// <summary>
/// Stub du service de notification
/// En attente d'intégration avec le Notification.Service
/// </summary>
public class NotificationServiceStub : INotificationService
{
    /// <summary>
    /// Envoie une notification de candidature créée
    /// </summary>
    public async Task NotifyCandidatureCreatedAsync(Guid candidatureId, int stagiaireId)
    {
        // TODO: Intégrer avec Notification.Service via HTTP ou Event Bus
        // Pour le moment, juste un log
        await Task.CompletedTask;
        System.Diagnostics.Debug.WriteLine($"[NOTIFICATION] Candidature {candidatureId} créée par stagiaire {stagiaireId}");
    }

    public async Task NotifyCandidatureAcceptedAsync(Guid candidatureId, int stagiaireId, string emailStagiaire, string nomStagiaire)
    {
        await Task.CompletedTask;
    }

    public async Task NotifyAcceptedByDirectionAsync(Guid candidatureId, int stagiaireId, int encadrantId, string emailStagiaire, string nomStagiaire, string token)
    {
        await Task.CompletedTask;
    }

    /// <summary>
    /// Envoie une notification de refus
    /// </summary>
    public async Task NotifyCandidatureRejectedAsync(Guid candidatureId, int stagiaireId, string commentaire)
    {
        // TODO: Intégrer avec Notification.Service
        await Task.CompletedTask;
        System.Diagnostics.Debug.WriteLine($"[NOTIFICATION] Candidature {candidatureId} refusée pour stagiaire {stagiaireId}. Raison: {commentaire}");
    }

    /// <summary>
    /// Envoie une notification de transmission à la Direction
    /// </summary>
    public async Task NotifyTransmittedToDirectionAsync(Guid candidatureId)
    {
        // TODO: Intégrer avec Notification.Service
        await Task.CompletedTask;
        System.Diagnostics.Debug.WriteLine($"[NOTIFICATION] Candidature {candidatureId} transmise à la Direction");
    }
}
