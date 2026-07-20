using Microsoft.EntityFrameworkCore;
using Notifications.Domain.Entities;
using Notifications.Domain.Interfaces;
using Notifications.Infrastructure.Persistence;

namespace Notifications.Infrastructure.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly NotificationsDbContext _context;

    public NotificationRepository(NotificationsDbContext context)
    {
        _context = context;
    }

    public async Task<Notification> AddAsync(Notification notification)
    {
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
        return notification;
    }

    public async Task<IEnumerable<Notification>> GetByDestinataireAsync(int destinataireId)
        => await _context.Notifications
            .Where(n => n.DestinataireId == destinataireId)
            .OrderByDescending(n => n.DateCreation)
            .Take(50)
            .ToListAsync();

    public async Task<int> GetUnreadCountAsync(int destinataireId)
        => await _context.Notifications
            .CountAsync(n => n.DestinataireId == destinataireId && !n.EstLue);

    public async Task MarkAsReadAsync(Guid notificationId)
    {
        var notif = await _context.Notifications.FindAsync(notificationId);
        if (notif is not null)
        {
            notif.EstLue = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task MarkAllAsReadAsync(int destinataireId)
    {
        await _context.Notifications
            .Where(n => n.DestinataireId == destinataireId && !n.EstLue)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.EstLue, true));
    }
}
