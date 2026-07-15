namespace Candidatures.Application.Commands;

/// <summary>
/// Commande pour refuser une candidature
/// </summary>
public class RejectCandidatureCommand
{
    /// <summary>
    /// Identifiant de la candidature
    /// </summary>
    public Guid CandidatureId { get; set; }

    /// <summary>
    /// Identifiant de l'encadrant (int - matching Authentication.Service)
    /// </summary>
    public int EncadrantId { get; set; }

    /// <summary>
    /// Motif du refus (obligatoire)
    /// </summary>
    public string Commentaire { get; set; } = string.Empty;

    public RejectCandidatureCommand(Guid candidatureId, int encadrantId, string commentaire)
    {
        CandidatureId = candidatureId;
        EncadrantId = encadrantId;
        Commentaire = commentaire;
    }
}
