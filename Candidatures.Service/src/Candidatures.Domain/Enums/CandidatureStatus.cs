namespace Candidatures.Domain.Enums;

/// <summary>
/// Énumération des statuts d'une candidature
/// </summary>
public enum CandidatureStatus
{
    /// <summary>
    /// En attente de traitement par l'encadrant
    /// </summary>
    EnAttente = 0,

    /// <summary>
    /// Acceptée par l'encadrant
    /// </summary>
    Acceptee = 1,

    /// <summary>
    /// Refusée par l'encadrant
    /// </summary>
    Refusee = 2
}
