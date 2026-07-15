namespace Candidatures.Application.Queries;

/// <summary>
/// Requête pour récupérer le suivi d'une candidature
/// </summary>
public class GetCandidatureSuiviQuery
{
    /// <summary>
    /// Identifiant de la candidature
    /// </summary>
    public Guid CandidatureId { get; set; }

    public GetCandidatureSuiviQuery(Guid candidatureId)
    {
        CandidatureId = candidatureId;
    }
}
