using Notifications.Application.DTOs;
using Notifications.Application.Interfaces;
using Notifications.Domain.Entities;
using Notifications.Domain.Enums;
using Notifications.Domain.Interfaces;

namespace Notifications.Application.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repository;
    private readonly IEmailService _emailService;

    public NotificationService(
        INotificationRepository repository,
        IEmailService emailService)
    {
        _repository = repository;
        _emailService = emailService;
    }

    public async Task EnvoyerNotificationCandidatureAccepteeAsync(
        int stagiaireId, Guid candidatureId, string emailStagiaire, string nomStagiaire)
    {
        var notif = new Notification
        {
            DestinataireId = stagiaireId,
            Type = NotificationType.CandidatureAcceptee,
            Titre = "Votre candidature a été acceptée !",
            Message = "Félicitations ! Votre candidature a été acceptée par un encadrant. Veuillez envoyer vos documents dans la section Documents.",
            CandidatureId = candidatureId
        };
        await _repository.AddAsync(notif);

        if (!string.IsNullOrWhiteSpace(emailStagiaire))
        {
            try { await _emailService.EnvoyerEmailCandidatureAccepteeAsync(emailStagiaire, nomStagiaire); }
            catch { /* Email non bloquant */ }
        }
    }

    public async Task EnvoyerDemandeCorrectionsDocumentAsync(
        int stagiaireId, Guid documentId, string typeDocument, string commentaire)
    {
        var notif = new Notification
        {
            DestinataireId = stagiaireId,
            Type = NotificationType.CorrectionDocumentDemandee,
            Titre = $"Correction demandée : {typeDocument}",
            Message = $"Une correction est demandée pour votre document \"{typeDocument}\". Commentaire : {commentaire}",
            DocumentId = documentId
        };
        await _repository.AddAsync(notif);
    }

    public async Task<IEnumerable<NotificationDto>> GetNotificationsAsync(int destinataireId)
    {
        var notifs = await _repository.GetByDestinataireAsync(destinataireId);
        return notifs.Select(n => new NotificationDto
        {
            Id = n.Id,
            Type = n.Type.ToString(),
            Titre = n.Titre,
            Message = n.Message,
            EstLue = n.EstLue,
            DateCreation = n.DateCreation,
            CandidatureId = n.CandidatureId,
            DocumentId = n.DocumentId
        }).OrderByDescending(n => n.DateCreation);
    }

    public async Task<int> GetUnreadCountAsync(int destinataireId)
        => await _repository.GetUnreadCountAsync(destinataireId);

    public async Task MarquerCommeLueAsync(Guid notificationId)
        => await _repository.MarkAsReadAsync(notificationId);

    public async Task MarquerToutesCommeLuesAsync(int destinataireId)
        => await _repository.MarkAllAsReadAsync(destinataireId);
}
