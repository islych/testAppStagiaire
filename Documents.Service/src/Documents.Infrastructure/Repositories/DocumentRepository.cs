using Documents.Domain.Entities;
using Documents.Domain.Enums;
using Documents.Domain.Interfaces;
using Documents.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Documents.Infrastructure.Repositories;

/// <summary>
/// Implémentation du repository pour les documents
/// </summary>
public class DocumentRepository : IDocumentRepository
{
    private readonly DocumentsDbContext _context;

    public DocumentRepository(DocumentsDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Récupère un document par son ID
    /// </summary>
    public async Task<Document?> GetByIdAsync(Guid id)
    {
        return await _context.Documents
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    /// <summary>
    /// Récupère tous les documents (version courante) d'un stagiaire
    /// </summary>
    public async Task<IEnumerable<Document>> GetByStagiaireIdAsync(int stagiaireId)
    {
        return await _context.Documents
            .AsNoTracking()
            .Where(d => d.StagiaireId == stagiaireId && d.EstVersionCourante)
            .OrderByDescending(d => d.DateDepot)
            .ToListAsync();
    }

    /// <summary>
    /// Récupère les documents (version courante) liés à une candidature
    /// </summary>
    public async Task<IEnumerable<Document>> GetByCandidatureIdAsync(Guid candidatureId)
    {
        return await _context.Documents
            .AsNoTracking()
            .Where(d => d.CandidatureId == candidatureId && d.EstVersionCourante)
            .OrderByDescending(d => d.DateDepot)
            .ToListAsync();
    }

    /// <summary>
    /// Récupère les documents par statut (version courante uniquement)
    /// </summary>
    public async Task<IEnumerable<Document>> GetByStatutAsync(DocumentStatut statut)
    {
        return await _context.Documents
            .AsNoTracking()
            .Where(d => d.Statut == statut && d.EstVersionCourante)
            .OrderByDescending(d => d.DateDepot)
            .ToListAsync();
    }

    /// <summary>
    /// Récupère les documents par type (version courante uniquement)
    /// </summary>
    public async Task<IEnumerable<Document>> GetByTypeAsync(TypeDocument type)
    {
        return await _context.Documents
            .AsNoTracking()
            .Where(d => d.Type == type && d.EstVersionCourante)
            .OrderByDescending(d => d.DateDepot)
            .ToListAsync();
    }

    /// <summary>
    /// Récupère les documents (version courante) d'un stagiaire filtrés par type
    /// </summary>
    public async Task<IEnumerable<Document>> GetByStagiaireAndTypeAsync(int stagiaireId, TypeDocument type)
    {
        return await _context.Documents
            .AsNoTracking()
            .Where(d => d.StagiaireId == stagiaireId && d.Type == type && d.EstVersionCourante)
            .OrderByDescending(d => d.DateDepot)
            .ToListAsync();
    }

    /// <summary>
    /// Récupère tous les documents en version courante
    /// </summary>
    public async Task<IEnumerable<Document>> GetAllCurrentVersionsAsync()
    {
        return await _context.Documents
            .AsNoTracking()
            .Where(d => d.EstVersionCourante)
            .OrderByDescending(d => d.DateDepot)
            .ToListAsync();
    }

    /// <summary>
    /// Crée un nouveau document
    /// </summary>
    public async Task<Document> CreateAsync(Document document)
    {
        _context.Documents.Add(document);
        await _context.SaveChangesAsync();
        return document;
    }

    /// <summary>
    /// Met à jour un document existant
    /// </summary>
    public async Task<Document> UpdateAsync(Document document)
    {
        _context.Documents.Update(document);
        await _context.SaveChangesAsync();
        return document;
    }

    /// <summary>
    /// Supprime un document par son ID
    /// </summary>
    public async Task<bool> DeleteAsync(Guid id)
    {
        var document = await _context.Documents.FindAsync(id);
        if (document == null)
            return false;

        _context.Documents.Remove(document);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Vérifie si un document existe
    /// </summary>
    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Documents
            .AsNoTracking()
            .AnyAsync(d => d.Id == id);
    }

    /// <summary>
    /// Récupère les documents (version courante) destinés à un acteur précis
    /// </summary>
    public async Task<IEnumerable<Document>> GetByDestinataireAsync(string destinataire)
    {
        return await _context.Documents
            .AsNoTracking()
            .Where(d => d.DestinataireActuel == destinataire && d.EstVersionCourante)
            .OrderByDescending(d => d.DateDepot)
            .ToListAsync();
    }

    /// <summary>
    /// Récupère les documents (version courante) d'une candidature pour un destinataire précis
    /// </summary>
    public async Task<IEnumerable<Document>> GetByCandidatureAndDestinataireAsync(Guid candidatureId, string destinataire)
    {
        return await _context.Documents
            .AsNoTracking()
            .Where(d => d.CandidatureId == candidatureId && d.DestinataireActuel == destinataire && d.EstVersionCourante)
            .OrderByDescending(d => d.DateDepot)
            .ToListAsync();
    }
}
