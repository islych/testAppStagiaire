namespace Documents.Application.Queries;

/// <summary>
/// Requête pour récupérer tous les documents liés à une candidature
/// </summary>
public record GetDocumentsByCandidatureQuery(Guid CandidatureId);
