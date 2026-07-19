namespace Documents.Domain.Exceptions;

/// <summary>
/// Exception levée lorsqu'un document est introuvable
/// </summary>
public class DocumentNotFoundException : Exception
{
    public DocumentNotFoundException(Guid id)
        : base($"Le document avec l'identifiant '{id}' est introuvable.") { }
}
