using Documents.Domain.Entities;
using Documents.Domain.Enums;

namespace Documents.Domain.Interfaces;

/// <summary>
/// Contrat du repository pour les documents
/// </summary>
public interface IDocumentRepository
{
    /// <summary>Récupère un document par son ID</summary>
    Task<Document?> GetByIdAsync(Guid id);

    /// <summary>Récupère tous les documents d'un stagiaire</summary>
    Task<IEnumerable<Document>> GetByStagiaireIdAsync(int stagiaireId);

    /// <summary>Récupère les documents d'une candidature</summary>
    Task<IEnumerable<Document>> GetByCandidatureIdAsync(Guid candidatureId);

    /// <summary>Récupère les documents par statut</summary>
    Task<IEnumerable<Document>> GetByStatutAsync(DocumentStatut statut);

    /// <summary>Récupère les documents par type</summary>
    Task<IEnumerable<Document>> GetByTypeAsync(TypeDocument type);

    /// <summary>Récupère les documents d'un stagiaire filtrés par type</summary>
    Task<IEnumerable<Document>> GetByStagiaireAndTypeAsync(int stagiaireId, TypeDocument type);

    /// <summary>Récupère tous les documents (version courante uniquement)</summary>
    Task<IEnumerable<Document>> GetAllCurrentVersionsAsync();

    /// <summary>Crée un nouveau document</summary>
    Task<Document> CreateAsync(Document document);

    /// <summary>Met à jour un document existant</summary>
    Task<Document> UpdateAsync(Document document);

    /// <summary>Supprime un document</summary>
    Task<bool> DeleteAsync(Guid id);

    /// <summary>Récupère les documents destinés à un acteur précis (Encadrant ou Centre)</summary>
    Task<IEnumerable<Document>> GetByDestinataireAsync(string destinataire);

    /// <summary>Récupère les documents d'une candidature destinés à un acteur précis</summary>
    Task<IEnumerable<Document>> GetByCandidatureAndDestinataireAsync(Guid candidatureId, string destinataire);
}
