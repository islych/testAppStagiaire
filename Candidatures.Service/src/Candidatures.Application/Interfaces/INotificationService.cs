namespace Candidatures.Application.Interfaces;

/// <summary>
/// Interface pour la communication avec le service de notification
/// Permet d'envoyer des notifications sans dépendre directement du service
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Envoie une notification de candidature créée
    /// </summary>
    Task NotifyCandidatureCreatedAsync(Guid candidatureId, int stagiaireId);

    /// <summary>
    /// Envoie une notification d'acceptation
    /// </summary>
    Task NotifyCandidatureAcceptedAsync(Guid candidatureId, int stagiaireId);

    /// <summary>
    /// Envoie une notification de refus
    /// </summary>
    Task NotifyCandidatureRejectedAsync(Guid candidatureId, int stagiaireId, string commentaire);

    /// <summary>
    /// Envoie une notification de transmission à la Direction
    /// </summary>
    Task NotifyTransmittedToDirectionAsync(Guid candidatureId);
}
