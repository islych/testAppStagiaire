namespace Documents.Application.Interfaces;

/// <summary>
/// Contrat du service de notifications
/// </summary>
public interface INotificationService
{
    /// <summary>Notifie le stagiaire qu'un document a été validé</summary>
    Task NotifyDocumentValideAsync(Guid documentId, int stagiaireId);

    /// <summary>Notifie le stagiaire qu'un document a été refusé</summary>
    Task NotifyDocumentRefuseAsync(Guid documentId, int stagiaireId, string commentaire);

    /// <summary>Notifie l'encadrant qu'un nouveau document a été déposé</summary>
    Task NotifyNouveauDocumentAsync(Guid documentId, int stagiaireId);

    /// <summary>Notifie l'encadrant qu'une correction a été soumise</summary>
    Task NotifyCorrectionSoumiseAsync(Guid documentId, int stagiaireId);
}
