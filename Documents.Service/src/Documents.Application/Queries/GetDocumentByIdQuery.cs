namespace Documents.Application.Queries;

/// <summary>
/// Requête pour récupérer un document par son ID
/// </summary>
public record GetDocumentByIdQuery(Guid DocumentId);
