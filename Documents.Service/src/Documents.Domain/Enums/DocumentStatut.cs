namespace Documents.Domain.Enums;

/// <summary>
/// Statut d'un document dans le cycle de validation
/// </summary>
public enum DocumentStatut
{
    /// <summary>Document déposé, en attente de vérification</summary>
    EnAttente,

    /// <summary>Document validé par le vérificateur</summary>
    Valide,

    /// <summary>Document refusé, correction demandée au stagiaire</summary>
    Refuse,

    /// <summary>Nouvelle version soumise après un refus</summary>
    EnCorrectionSoumise
}
