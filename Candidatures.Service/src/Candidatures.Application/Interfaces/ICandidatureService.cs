using Candidatures.Application.DTOs;

namespace Candidatures.Application.Interfaces;

public interface ICandidatureService
{
    Task<CandidatureDto> CreateCandidatureAsync(CreateCandidatureDto dto);
    Task<CandidatureDto?> GetCandidatureByIdAsync(Guid id);
    Task<IEnumerable<CandidatureDto>> GetAllCandidaturesAsync();
    Task<IEnumerable<CandidatureDto>> GetCandidaturesByDepartementAsync(int departementId);
    Task<IEnumerable<CandidatureDto>> GetCandidaturesByStagiaireAsync(int stagiaireId);

    /// <summary>Encadrant transmet la candidature à la Direction</summary>
    Task<CandidatureDto> TransmettreADirectionAsync(Guid candidatureId, int encadrantId);

    /// <summary>Direction transmet la candidature au Centre</summary>
    Task<CandidatureDto> TransmettreCentreAsync(Guid candidatureId);

    /// <summary>Centre transmet la candidature acceptée au RH</summary>
    Task<CandidatureDto> TransmettreRHAsync(Guid candidatureId);

    /// <summary>RH intègre le stagiaire dans le système (étape finale)</summary>
    Task<CandidatureDto> IntegrerStagiaireAsync(Guid candidatureId);

    /// <summary>Encadrant refuse directement une candidature</summary>
    Task<CandidatureDto> RejectCandidatureAsync(Guid candidatureId, int encadrantId, string commentaire);

    /// <summary>Direction accepte → notifie stagiaire + encadrant par email</summary>
    Task<CandidatureDto> AccepterParDirectionAsync(Guid candidatureId, string token);

    /// <summary>Direction refuse la candidature</summary>
    Task<CandidatureDto> RefuserParDirectionAsync(Guid candidatureId, string commentaire);

    Task<CandidatureSuiviDto?> GetCandidatureSuiviAsync(Guid candidatureId);

    /// <summary>
    /// Documents.Service appelle cet endpoint quand tous les documents sont validés.
    /// Met DestinataireTransmission = "DossierAccepte".
    /// </summary>
    Task<CandidatureDto> MarquerDossierAccepteAsync(Guid candidatureId);
}
