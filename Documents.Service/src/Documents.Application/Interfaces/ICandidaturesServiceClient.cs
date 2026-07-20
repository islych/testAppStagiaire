namespace Documents.Application.Interfaces;

/// <summary>
/// Contrat du client HTTP vers Candidatures.Service.
/// Permet au Documents.Service de vérifier le statut d'une candidature
/// avant d'autoriser le dépôt de documents.
/// </summary>
public interface ICandidaturesServiceClient
{
    /// <summary>
    /// Vérifie si le stagiaire possède au moins une candidature acceptée par un encadrant.
    /// Retourne true uniquement si au moins une candidature a le statut "Acceptee".
    /// </summary>
    Task<bool> StagiaireACandidatureAccepteeAsync(int stagiaireId, string jwtToken);

    /// <summary>
    /// Notifie Candidatures.Service que tous les documents d'un dossier ont été validés.
    /// Déclenche le passage automatique du dossier en "DossierAccepte".
    /// </summary>
    Task MarquerDossierAccepteAsync(Guid candidatureId, string jwtToken);
}
