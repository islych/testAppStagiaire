namespace Candidatures.Application.Commands;

/// <summary>
/// Commande pour accepter une candidature
/// </summary>
public class AcceptCandidatureCommand
{
    /// <summary>
    /// Identifiant de la candidature
    /// </summary>
    public Guid CandidatureId { get; set; }

    /// <summary>
    /// Identifiant de l'encadrant (int - matching Authentication.Service)
    /// </summary>
    public int EncadrantId { get; set; }

    public AcceptCandidatureCommand(Guid candidatureId, int encadrantId)
    {
        CandidatureId = candidatureId;
        EncadrantId = encadrantId;
    }
}
