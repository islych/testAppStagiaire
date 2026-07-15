using Candidatures.Application.DTOs;

namespace Candidatures.Application.Interfaces;

/// <summary>
/// Interface du service métier des candidatures
/// </summary>
public interface ICandidatureService
{
    /// <summary>
    /// Crée une nouvelle candidature
    /// </summary>
    Task<CandidatureDto> CreateCandidatureAsync(CreateCandidatureDto dto);

    /// <summary>
    /// Récupère une candidature par son ID
    /// </summary>
    Task<CandidatureDto?> GetCandidatureByIdAsync(Guid id);

    /// <summary>
    /// Récupère toutes les candidatures
    /// </summary>
    Task<IEnumerable<CandidatureDto>> GetAllCandidaturesAsync();

    /// <summary>
    /// Récupère les candidatures d'un stagiaire (int - matching Authentication.Service)
    /// </summary>
    Task<IEnumerable<CandidatureDto>> GetCandidaturesByStagiaireAsync(int stagiaireId);

    /// <summary>
    /// Accepte une candidature
    /// </summary>
    Task<CandidatureDto> AcceptCandidatureAsync(Guid candidatureId, int encadrantId);

    /// <summary>
    /// Refuse une candidature
    /// </summary>
    Task<CandidatureDto> RejectCandidatureAsync(Guid candidatureId, int encadrantId, string commentaire);

    /// <summary>
    /// Récupère le suivi d'une candidature
    /// </summary>
    Task<CandidatureSuiviDto?> GetCandidatureSuiviAsync(Guid candidatureId);

    /// <summary>
    /// Transmet une candidature acceptée à la Direction
    /// </summary>
    Task<bool> TransmitToDirectionAsync(Guid candidatureId);
}
