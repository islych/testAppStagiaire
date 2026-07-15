using GestionDesStagiaires.Web.Models;

namespace GestionDesStagiaires.Web.Services;

/// <summary>
/// Interface du service Candidatures API
/// </summary>
public interface ICandidaturesApiService
{
    /// <summary>
    /// Crée une nouvelle candidature
    /// </summary>
    Task<ApiResponse<CandidatureViewModel>> CreateCandidatureAsync(CreateCandidatureViewModel candidature, string token);

    /// <summary>
    /// Récupère une candidature par son ID
    /// </summary>
    Task<ApiResponse<CandidatureViewModel>> GetCandidatureByIdAsync(Guid id, string token);

    /// <summary>
    /// Récupère toutes les candidatures
    /// </summary>
    Task<ApiResponse<IEnumerable<CandidatureViewModel>>> GetAllCandidaturesAsync(string token);

    /// <summary>
    /// Récupère les candidatures du stagiaire connecté
    /// </summary>
    Task<ApiResponse<IEnumerable<CandidatureViewModel>>> GetMyCandidaturesAsync(string token);

    /// <summary>
    /// Récupère le suivi d'une candidature
    /// </summary>
    Task<ApiResponse<CandidatureSuiviViewModel>> GetCandidatureSuiviAsync(Guid id, string token);

    /// <summary>
    /// Accepte une candidature
    /// </summary>
    Task<ApiResponse<CandidatureViewModel>> AcceptCandidatureAsync(Guid id, string token);

    /// <summary>
    /// Refuse une candidature
    /// </summary>
    Task<ApiResponse<CandidatureViewModel>> RejectCandidatureAsync(Guid id, string motif, string token);
}
