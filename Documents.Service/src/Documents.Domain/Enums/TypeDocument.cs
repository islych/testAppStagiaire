namespace Documents.Domain.Enums;

/// <summary>
/// Types de documents administratifs gérés par la plateforme
/// </summary>
public enum TypeDocument
{
    /// <summary>Curriculum Vitae du stagiaire</summary>
    CV,

    /// <summary>Convention de stage signée</summary>
    ConventionDeStage,

    /// <summary>Attestation d'assurance</summary>
    Assurance,

    /// <summary>Carte d'identité nationale</summary>
    CIN,

    /// <summary>Lettre de recommandation</summary>
    LettreDeRecommandation,

    /// <summary>Rapport de stage final</summary>
    RapportDeStage,

    /// <summary>Attestation de stage délivrée par le centre</summary>
    AttestationDeStage,

    /// <summary>Demande manuscrite</summary>
    DemandeManuscrite,

    /// <summary>Fiche d'appréciation de l'encadrant</summary>
    FicheAppreciation,

    /// <summary>Fiche de synthèse</summary>
    FicheDeSynthese
}
