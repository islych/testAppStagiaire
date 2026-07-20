using Documents.Application.DTOs;
using Documents.Domain.Enums;

namespace Documents.Application.Interfaces;

/// <summary>
/// Contrat du service applicatif pour la gestion des documents
/// </summary>
public interface IDocumentService
{
    /// <summary>
    /// Upload un nouveau document pour un stagiaire.
    /// Lève une ApplicationException si le stagiaire n'a pas de candidature acceptée.
    /// </summary>
    Task<DocumentDto> UploadDocumentAsync(
        int stagiaireId,
        Guid? candidatureId,
        TypeDocument type,
        string nomFichier,
        string nomFichierStockage,
        string cheminFichier,
        string extension,
        long tailleFichierOctets,
        string contentType,
        string jwtToken);

    /// <summary>Soumet une correction (nouvelle version) pour un document en DemandeModification — va directement au Centre</summary>
    Task<DocumentDto> SoumettreCorrectionsAsync(
        Guid documentOriginalId,
        int stagiaireId,
        TypeDocument type,
        string nomFichier,
        string nomFichierStockage,
        string cheminFichier,
        string extension,
        long tailleFichierOctets,
        string contentType);

    /// <summary>L'encadrant transmet tous les documents d'un stagiaire au Centre (par candidatureId)</summary>
    Task<IEnumerable<DocumentDto>> TransmettreAuCentreAsync(Guid candidatureId, int encadrantId);

    /// <summary>Valide un document (Centre uniquement après transmission)</summary>
    Task<DocumentDto> ValiderDocumentAsync(Guid documentId, int verificateurId, string? commentaire, string jwtToken);

    /// <summary>Refuse définitivement un document (Centre uniquement)</summary>
    Task<DocumentDto> RefuserDocumentAsync(Guid documentId, int verificateurId, string commentaireRefus);

    /// <summary>Le Centre demande des modifications avec un commentaire — stagiaire corrige et renvoie directement au Centre</summary>
    Task<DocumentDto> DemanderModificationAsync(Guid documentId, int centreUserId, string commentaire);

    /// <summary>Récupère un document par son ID</summary>
    Task<DocumentDto?> GetDocumentByIdAsync(Guid id);

    /// <summary>Récupère tous les documents d'un stagiaire</summary>
    Task<IEnumerable<DocumentDto>> GetDocumentsByStagiaireAsync(int stagiaireId);

    /// <summary>Récupère les documents d'une candidature</summary>
    Task<IEnumerable<DocumentDto>> GetDocumentsByCandidatureAsync(Guid candidatureId);

    /// <summary>Récupère tous les documents (version courante)</summary>
    Task<IEnumerable<DocumentDto>> GetAllDocumentsAsync();

    /// <summary>Récupère les documents par statut</summary>
    Task<IEnumerable<DocumentDto>> GetDocumentsByStatutAsync(DocumentStatut statut);

    /// <summary>Supprime un document EnAttente (stagiaire propriétaire uniquement)</summary>
    Task SupprimerDocumentAsync(Guid documentId, int stagiaireId);
}
