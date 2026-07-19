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

    /// <summary>Soumet une correction (nouvelle version) pour un document refusé</summary>
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

    /// <summary>Valide un document (Encadrant / RH / Centre)</summary>
    Task<DocumentDto> ValiderDocumentAsync(Guid documentId, int verificateurId, string? commentaire);

    /// <summary>Refuse un document avec un commentaire obligatoire</summary>
    Task<DocumentDto> RefuserDocumentAsync(Guid documentId, int verificateurId, string commentaireRefus);

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
}
