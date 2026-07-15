namespace Candidatures.Application.Queries;

/// <summary>
/// Requête pour récupérer les candidatures d'un stagiaire
/// </summary>
public class GetCandidaturesByStagiaireQuery
{
    /// <summary>
    /// Identifiant du stagiaire
    /// </summary>
    public Guid StagiaireId { get; set; }

    public GetCandidaturesByStagiaireQuery(Guid stagiaireId)
    {
        StagiaireId = stagiaireId;
    }
}
