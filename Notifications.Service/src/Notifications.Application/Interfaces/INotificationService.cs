using Notifications.Application.DTOs;

namespace Notifications.Application.Interfaces;

public interface INotificationService
{
    Task EnvoyerNotificationCandidatureAccepteeAsync(int stagiaireId, Guid candidatureId, string emailStagiaire, string nomStagiaire);
    Task EnvoyerDemandeCorrectionsDocumentAsync(int stagiaireId, Guid documentId, string typeDocument, string commentaire);
    Task<IEnumerable<NotificationDto>> GetNotificationsAsync(int destinataireId);
    Task<int> GetUnreadCountAsync(int destinataireId);
    Task MarquerCommeLueAsync(Guid notificationId);
    Task MarquerToutesCommeLuesAsync(int destinataireId);
}
