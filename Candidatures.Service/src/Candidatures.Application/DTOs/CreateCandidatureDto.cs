using Candidatures.Domain.Enums;

namespace Candidatures.Application.DTOs;

/// <summary>
/// DTO pour la création d'une candidature
/// </summary>
public class CreateCandidatureDto
{
    /// <summary>
    /// Identifiant du stagiaire (int - matching Authentication.Service)
    /// </summary>
    public int StagiaireId { get; set; }

    /// <summary>
    /// Identifiant du département choisi
    /// </summary>
    public int DepartementId { get; set; }

    /// <summary>
    /// Identifiant de la spécialité choisie
    /// </summary>
    public int SpecialiteId { get; set; }

    /// <summary>
    /// Durée du stage souhaitée en mois (1-6 mois)
    /// </summary>
    public int DureeStageMois { get; set; } = 3;

    /// <summary>
    /// Date de début du stage souhaitée
    /// </summary>
    public DateTime DateDebut { get; set; }

    /// <summary>
    /// Date de fin du stage souhaitée
    /// </summary>
    public DateTime DateFin { get; set; }

    /// <summary>
    /// Niveau d'études du stagiaire
    /// </summary>
    public NiveauEtude NiveauEtude { get; set; } = NiveauEtude.BacPlus3;

    /// <summary>
    /// Nom de l'école/université d'origine
    /// </summary>
    public string Ecole { get; set; } = string.Empty;

    /// <summary>
    /// Lettre de motivation (texte)
    /// </summary>
    public string LettreMotivation { get; set; } = string.Empty;

    /// <summary>
    /// Nom du fichier CV
    /// </summary>
    public string CvFileName { get; set; } = string.Empty;

    /// <summary>
    /// Chemin ou URL du fichier CV
    /// </summary>
    public string CvPath { get; set; } = string.Empty;
}
