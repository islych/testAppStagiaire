namespace Notifications.Application.DTOs;

public class NotificationDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Titre { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool EstLue { get; set; }
    public DateTime DateCreation { get; set; }
    public Guid? CandidatureId { get; set; }
    public Guid? DocumentId { get; set; }
}
