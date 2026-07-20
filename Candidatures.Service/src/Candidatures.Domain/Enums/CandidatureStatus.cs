namespace Candidatures.Domain.Enums;

public enum CandidatureStatus
{
    /// <summary>En attente de traitement par l'encadrant</summary>
    EnAttente = 0,

    /// <summary>Transmise à la Direction par l'encadrant</summary>
    TransmiseADirection = 1,

    /// <summary>Acceptée par la Direction</summary>
    Acceptee = 2,

    /// <summary>Refusée (par l'encadrant ou la Direction)</summary>
    Refusee = 3
}
