using GestionDesStagiaires.Web.Models;

namespace GestionDesStagiaires.Web.Services;

/// <summary>
/// Interface du service API pour les données maîtres (départements, spécialités, etc.)
/// </summary>
public interface IMasterDataApiService
{
    /// <summary>
    /// Récupère la liste de tous les départements
    /// </summary>
    Task<ApiResponse<IEnumerable<DepartementOptionResponse>>> GetDepartementsAsync(string token);

    /// <summary>
    /// Récupère les spécialités d'un département
    /// </summary>
    Task<ApiResponse<IEnumerable<SpecialiteOptionResponse>>> GetSpecialitesByDepartementAsync(int departementId, string token);

    /// <summary>
    /// Récupère un département par son ID
    /// </summary>
    Task<ApiResponse<DepartementOptionResponse>> GetDepartementByIdAsync(int departementId, string token);

    /// <summary>
    /// Récupère une spécialité par son ID
    /// </summary>
    Task<ApiResponse<SpecialiteOptionResponse>> GetSpecialiteByIdAsync(int specialiteId, string token);
}
