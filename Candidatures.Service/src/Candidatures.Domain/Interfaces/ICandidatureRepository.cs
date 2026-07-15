using Candidatures.Domain.Entities;
using Candidatures.Domain.Enums;

namespace Candidatures.Domain.Interfaces;

/// <summary>
/// Interface du repository pour les candidatures
/// </summary>
public interface ICandidatureRepository
{
    /// <summary>
    /// Récupère une candidature par son ID
    /// </summary>
    Task<Candidature?> GetByIdAsync(Guid id);

    /// <summary>
    /// Récupère toutes les candidatures d'un stagiaire
    /// </summary>
    Task<IEnumerable<Candidature>> GetByStagiaireIdAsync(int stagiaireId);

    /// <summary>
    /// Récupère toutes les candidatures en attente ou traitées
    /// </summary>
    Task<IEnumerable<Candidature>> GetAllAsync();

    /// <summary>
    /// Récupère les candidatures par statut
    /// </summary>
    Task<IEnumerable<Candidature>> GetByStatutAsync(CandidatureStatus statut);

    /// <summary>
    /// Crée une nouvelle candidature
    /// </summary>
    Task<Candidature> CreateAsync(Candidature candidature);

    /// <summary>
    /// Met à jour une candidature existante
    /// </summary>
    Task<Candidature> UpdateAsync(Candidature candidature);

    /// <summary>
    /// Supprime une candidature
    /// </summary>
    Task<bool> DeleteAsync(Guid id);

    /// <summary>
    /// Vérifie si une candidature existe
    /// </summary>
    Task<bool> ExistsAsync(Guid id);

    /// <summary>
    /// Récupère les candidatures assignées à un encadrant
    /// </summary>
    Task<IEnumerable<Candidature>> GetByEncadrantIdAsync(int encadrantId);
}
