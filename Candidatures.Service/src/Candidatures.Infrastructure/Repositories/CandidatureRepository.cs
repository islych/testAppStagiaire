using Candidatures.Domain.Entities;
using Candidatures.Domain.Enums;
using Candidatures.Domain.Interfaces;
using Candidatures.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Candidatures.Infrastructure.Repositories;

/// <summary>
/// Repository pour les candidatures
/// </summary>
public class CandidatureRepository : ICandidatureRepository
{
    private readonly CandidaturesDbContext _context;

    public CandidatureRepository(CandidaturesDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Récupère une candidature par son ID
    /// </summary>
    public async Task<Candidature?> GetByIdAsync(Guid id)
    {
        return await _context.Candidatures
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    /// <summary>
    /// Récupère toutes les candidatures d'un stagiaire
    /// </summary>
    public async Task<IEnumerable<Candidature>> GetByStagiaireIdAsync(int stagiaireId)
    {
        return await _context.Candidatures
            .AsNoTracking()
            .Where(c => c.StagiaireId == stagiaireId)
            .OrderByDescending(c => c.DateCreation)
            .ToListAsync();
    }

    /// <summary>
    /// Récupère toutes les candidatures
    /// </summary>
    public async Task<IEnumerable<Candidature>> GetAllAsync()
    {
        return await _context.Candidatures
            .AsNoTracking()
            .OrderByDescending(c => c.DateCreation)
            .ToListAsync();
    }

    /// <summary>
    /// Récupère les candidatures par statut
    /// </summary>
    public async Task<IEnumerable<Candidature>> GetByStatutAsync(CandidatureStatus statut)
    {
        return await _context.Candidatures
            .AsNoTracking()
            .Where(c => c.Statut == statut)
            .OrderByDescending(c => c.DateCreation)
            .ToListAsync();
    }

    /// <summary>
    /// Crée une nouvelle candidature
    /// </summary>
    public async Task<Candidature> CreateAsync(Candidature candidature)
    {
        _context.Candidatures.Add(candidature);
        await _context.SaveChangesAsync();
        return candidature;
    }

    /// <summary>
    /// Met à jour une candidature existante
    /// </summary>
    public async Task<Candidature> UpdateAsync(Candidature candidature)
    {
        _context.Candidatures.Update(candidature);
        await _context.SaveChangesAsync();
        return candidature;
    }

    /// <summary>
    /// Supprime une candidature
    /// </summary>
    public async Task<bool> DeleteAsync(Guid id)
    {
        var candidature = await _context.Candidatures.FindAsync(id);
        if (candidature == null)
            return false;

        _context.Candidatures.Remove(candidature);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Vérifie si une candidature existe
    /// </summary>
    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Candidatures
            .AsNoTracking()
            .AnyAsync(c => c.Id == id);
    }

    /// <summary>
    /// Récupère les candidatures assignées à un encadrant
    /// </summary>
    public async Task<IEnumerable<Candidature>> GetByEncadrantIdAsync(int encadrantId)
    {
        return await _context.Candidatures
            .AsNoTracking()
            .Where(c => c.EncadrantId == encadrantId)
            .OrderByDescending(c => c.DateCreation)
            .ToListAsync();
    }

    /// <summary>
    /// Récupère les candidatures d'un département
    /// </summary>
    public async Task<IEnumerable<Candidature>> GetByDepartementIdAsync(int departementId)
    {
        return await _context.Candidatures
            .AsNoTracking()
            .Where(c => c.DepartementId == departementId)
            .OrderByDescending(c => c.DateCreation)
            .ToListAsync();
    }
}
