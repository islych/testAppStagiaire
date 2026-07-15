namespace Candidatures.Application.Queries;

/// <summary>
/// Requête pour récupérer une candidature par son ID
/// </summary>
public class GetCandidatureByIdQuery
{
    /// <summary>
    /// Identifiant de la candidature
    /// </summary>
    public Guid Id { get; set; }

    public GetCandidatureByIdQuery(Guid id)
    {
        Id = id;
    }
}
