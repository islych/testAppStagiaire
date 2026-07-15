using Candidatures.Domain.Enums;

namespace Candidatures.Application.DTOs;

/// <summary>
/// DTO détaillé pour l'affichage complet d'une candidature
/// </summary>
public class CandidatureDetailDto
{
    public Guid Id { get; set; }
    
    public int StagiaireId { get; set; }
    public string StagiaireNom { get; set; } = string.Empty;
    public string StagiairePrenom { get; set; } = string.Empty;
    public string StagiaireEmail { get; set; } = string.Empty;

    public int DepartementId { get; set; }
    public string DepartementNom { get; set; } = string.Empty;

    public int SpecialiteId { get; set; }
    public string SpecialiteNom { get; set; } = string.Empty;

    // Informations de stage
    public int DureeStageMois { get; set; }
    public DateTime DateDebut { get; set; }
    public DateTime DateFin { get; set; }
    public int DureeSemainesEstimee => DateFin > DateDebut 
        ? (int)((DateFin - DateDebut).TotalDays / 7) 
        : 0;

    // Informations académiques
    public NiveauEtude NiveauEtude { get; set; }
    public string Ecole { get; set; } = string.Empty;

    // Documents
    public string CvFileName { get; set; } = string.Empty;
    public string CvPath { get; set; } = string.Empty;
    public string LettreMotivation { get; set; } = string.Empty;

    // Statut et suivi
    public string Statut { get; set; } = string.Empty;
    public int? EncadrantId { get; set; }
    public string? EncadrantNom { get; set; }
    public string? EncadrantPrenom { get; set; }
    public string? Commentaire { get; set; }

    // Dates
    public DateTime DateCreation { get; set; }
    public DateTime? DateMiseAJour { get; set; }
    public DateTime? DateDecision { get; set; }
    public bool TransmisADirection { get; set; }
    public DateTime? DateTransmissionDirection { get; set; }
}
