using Notifications.Domain.Entities;

namespace Notifications.Domain.Interfaces;

public interface INotificationRepository
{
    Task<Notification> AddAsync(Notification notification);
    Task<IEnumerable<Notification>> GetByDestinataireAsync(int destinataireId);
    Task<int> GetUnreadCountAsync(int destinataireId);
    Task MarkAsReadAsync(Guid notificationId);
    Task MarkAllAsReadAsync(int destinataireId);
}
