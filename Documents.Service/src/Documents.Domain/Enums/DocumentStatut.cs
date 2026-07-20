namespace Documents.Domain.Enums;

/// <summary>
/// Statut d'un document dans le cycle de validation
/// </summary>
public enum DocumentStatut
{
    /// <summary>Document déposé par le stagiaire, visible uniquement par l'encadrant</summary>
    EnAttente,

    /// <summary>Document transmis au Centre par l'encadrant — en attente de décision du Centre</summary>
    TransmisAuCentre,

    /// <summary>Le Centre demande des modifications — stagiaire doit resoumettre</summary>
    DemandeModification,

    /// <summary>Nouvelle version soumise par le stagiaire après demande de modification — va directement au Centre</summary>
    EnCorrectionSoumise,

    /// <summary>Document validé (accepté) par le Centre</summary>
    Valide,

    /// <summary>Document définitivement refusé par le Centre</summary>
    Refuse
}
