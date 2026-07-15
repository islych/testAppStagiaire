using Candidatures.Domain.Mappings;
using Candidatures.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Candidatures.Application.Services;

/// <summary>
/// Service pour gérer les données de référence (départements et spécialités)
/// Utilise les énums définis dans Candidatures.Domain.Enums comme source unique de vérité.
/// </summary>
public class MasterDataService : IMasterDataService
{
    private readonly ILogger<MasterDataService> _logger;

    public MasterDataService(ILogger<MasterDataService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Obtient tous les départements
    /// </summary>
    public async Task<IEnumerable<DepartementInfo>> GetDepartementsAsync()
    {
        _logger.LogInformation("Récupération de tous les départements");
        return await Task.FromResult(DepartementSpecialiteMapping.GetAllDepartements());
    }

    /// <summary>
    /// Obtient un département par son ID
    /// </summary>
    public async Task<DepartementInfo?> GetDepartementByIdAsync(int id)
    {
        _logger.LogInformation("Récupération du département {DepartementId}", id);
        return await Task.FromResult(DepartementSpecialiteMapping.GetDepartementById(id));
    }

    /// <summary>
    /// Obtient les spécialités d'un département
    /// </summary>
    public async Task<IEnumerable<SpecialiteInfo>> GetSpecialitesByDepartementAsync(int departementId)
    {
        _logger.LogInformation("Récupération des spécialités du département {DepartementId}", departementId);
        return await Task.FromResult(DepartementSpecialiteMapping.GetSpecialitesByDepartement(departementId));
    }

    /// <summary>
    /// Obtient une spécialité par son ID
    /// </summary>
    public async Task<SpecialiteInfo?> GetSpecialiteByIdAsync(int id)
    {
        _logger.LogInformation("Récupération de la spécialité {SpecialiteId}", id);
        return await Task.FromResult(DepartementSpecialiteMapping.GetSpecialiteById(id));
    }

    /// <summary>
    /// Valide que le département et la spécialité existent ensemble
    /// </summary>
    public async Task<bool> ValidateDepartementAndSpecialiteAsync(int departementId, int specialiteId)
    {
        _logger.LogInformation("Validation du département {DepartementId} et spécialité {SpecialiteId}", departementId, specialiteId);
        
        var isValid = DepartementSpecialiteMapping.IsSpecialiteValideForDepartement(departementId, specialiteId);
        
        if (!isValid)
        {
            _logger.LogWarning("La spécialité {SpecialiteId} n'appartient pas au département {DepartementId}", specialiteId, departementId);
        }

        return await Task.FromResult(isValid);
    }

    /// <summary>
    /// Obtient toutes les spécialités
    /// </summary>
    public async Task<IEnumerable<SpecialiteInfo>> GetAllSpecialitesAsync()
    {
        _logger.LogInformation("Récupération de toutes les spécialités");
        return await Task.FromResult(DepartementSpecialiteMapping.GetAllSpecialites());
    }
}
