namespace Documents.Application.Interfaces;

/// <summary>
/// Contrat du service de notifications
/// </summary>
public interface INotificationService
{
    /// <summary>Notifie le stagiaire qu'un document a été validé (accepté par le Centre)</summary>
    Task NotifyDocumentValideAsync(Guid documentId, int stagiaireId);

    /// <summary>Notifie le stagiaire qu'un document a été définitivement refusé</summary>
    Task NotifyDocumentRefuseAsync(Guid documentId, int stagiaireId, string commentaire);

    /// <summary>Notifie l'encadrant qu'un nouveau document a été déposé par le stagiaire</summary>
    Task NotifyNouveauDocumentAsync(Guid documentId, int stagiaireId);

    /// <summary>Notifie le Centre qu'un dossier lui a été transmis par l'encadrant</summary>
    Task NotifyDossierTransmisAuCentreAsync(Guid documentId, int stagiaireId);

    /// <summary>Notifie le stagiaire que le Centre demande des modifications avec un commentaire</summary>
    Task NotifyModificationDemandeeAsync(Guid documentId, int stagiaireId, string commentaire);

    /// <summary>Notifie le Centre qu'une correction a été soumise par le stagiaire</summary>
    Task NotifyCorrectionSoumiseCentreAsync(Guid documentId, int stagiaireId);
}
