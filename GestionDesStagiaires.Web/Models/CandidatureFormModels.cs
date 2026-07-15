using System.ComponentModel.DataAnnotations;

namespace GestionDesStagiaires.Web.Models;

/// <summary>
/// Modèle pour le formulaire de création de candidature (Vue)
/// </summary>
public class CreateCandidatureFormViewModel
{
    /// <summary>
    /// Département sélectionné
    /// </summary>
    [Required(ErrorMessage = "Veuillez sélectionner un département")]
    [Range(1, int.MaxValue, ErrorMessage = "Département invalide")]
    public int? DepartementId { get; set; }

    /// <summary>
    /// Spécialité sélectionnée
    /// </summary>
    [Required(ErrorMessage = "Veuillez sélectionner une spécialité")]
    [Range(1, int.MaxValue, ErrorMessage = "Spécialité invalide")]
    public int? SpecialiteId { get; set; }

    /// <summary>
    /// Durée du stage en mois (1-6 mois)
    /// </summary>
    [Required(ErrorMessage = "Veuillez sélectionner une durée")]
    [Range(1, 6, ErrorMessage = "La durée doit être entre 1 et 6 mois")]
    public int? DureeStageMois { get; set; }

    /// <summary>
    /// Date de début souhaitée
    /// </summary>
    [Required(ErrorMessage = "Veuillez sélectionner une date de début")]
    [DataType(DataType.Date, ErrorMessage = "Format de date invalide")]
    public DateTime? DateDebut { get; set; }

    /// <summary>
    /// Date de fin souhaitée
    /// </summary>
    [Required(ErrorMessage = "Veuillez sélectionner une date de fin")]
    [DataType(DataType.Date, ErrorMessage = "Format de date invalide")]
    public DateTime? DateFin { get; set; }

    /// <summary>
    /// Niveau d'études
    /// </summary>
    [Required(ErrorMessage = "Veuillez sélectionner votre niveau d'études")]
    public string? NiveauEtude { get; set; }

    /// <summary>
    /// Nom de l'école/université
    /// </summary>
    [Required(ErrorMessage = "Veuillez entrer le nom de votre école")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Le nom de l'école doit faire entre 2 et 100 caractères")]
    public string? Ecole { get; set; }

    /// <summary>
    /// Lettre de motivation
    /// </summary>
    [Required(ErrorMessage = "Veuillez entrer une lettre de motivation")]
    [StringLength(2000, MinimumLength = 50, ErrorMessage = "La lettre doit faire entre 50 et 2000 caractères")]
    public string? LettreMotivation { get; set; }

    /// <summary>
    /// Fichier CV
    /// </summary>
    [Required(ErrorMessage = "Veuillez télécharger votre CV")]
    public IFormFile? CvFile { get; set; }
}

/// <summary>
/// Réponse pour les options de spécialités (API)
/// </summary>
public class SpecialiteOptionResponse
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Réponse pour les options de département (API)
/// </summary>
public class DepartementOptionResponse
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int SpecialitesCount { get; set; }
}

/// <summary>
/// Requête d'upload de CV
/// </summary>
public class UploadCvRequest
{
    public IFormFile File { get; set; } = null!;
    public string DepartementId { get; set; } = string.Empty;
    public string SpecialiteId { get; set; } = string.Empty;
}

