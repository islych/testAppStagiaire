namespace Documents.Domain.Exceptions;

/// <summary>
/// Exception levée lorsqu'une transition de statut est invalide
/// </summary>
public class DocumentStatutInvalideException : Exception
{
    public DocumentStatutInvalideException(string message)
        : base(message) { }
}
