using Notifications.Domain.Enums;

namespace Notifications.Domain.Entities;

public class Notification
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int DestinataireId { get; set; }
    public NotificationType Type { get; set; }
    public string Titre { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool EstLue { get; set; } = false;
    public DateTime DateCreation { get; set; } = DateTime.UtcNow;
    public Guid? CandidatureId { get; set; }
    public Guid? DocumentId { get; set; }
}
