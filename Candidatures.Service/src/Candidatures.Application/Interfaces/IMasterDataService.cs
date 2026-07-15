using Candidatures.Domain.Mappings;

namespace Candidatures.Application.Interfaces;

/// <summary>
/// Interface pour le service de données de référence (départements et spécialités)
/// Source unique de vérité: les énums définis dans Candidatures.Domain.Enums
/// </summary>
public interface IMasterDataService
{
    /// <summary>
    /// Obtient tous les départements
    /// </summary>
    Task<IEnumerable<DepartementInfo>> GetDepartementsAsync();

    /// <summary>
    /// Obtient un département par son ID
    /// </summary>
    Task<DepartementInfo?> GetDepartementByIdAsync(int id);

    /// <summary>
    /// Obtient les spécialités d'un département
    /// </summary>
    Task<IEnumerable<SpecialiteInfo>> GetSpecialitesByDepartementAsync(int departementId);

    /// <summary>
    /// Obtient une spécialité par son ID
    /// </summary>
    Task<SpecialiteInfo?> GetSpecialiteByIdAsync(int id);

    /// <summary>
    /// Valide que le département et la spécialité existent ensemble
    /// </summary>
    Task<bool> ValidateDepartementAndSpecialiteAsync(int departementId, int specialiteId);

    /// <summary>
    /// Obtient toutes les spécialités
    /// </summary>
    Task<IEnumerable<SpecialiteInfo>> GetAllSpecialitesAsync();
}
