using Candidatures.Domain.Enums;

namespace GestionDesStagiaires.Web.Models;

/// <summary>
/// ViewModel pour l'affichage d'une candidature
/// </summary>
public class CandidatureViewModel
{
    public Guid Id { get; set; }
    public int StagiaireId { get; set; }
    public int DepartementId { get; set; }
    public int SpecialiteId { get; set; }
    public string DepartementNom { get; set; } = string.Empty;
    public string SpecialiteNom { get; set; } = string.Empty;
    public int DureeStageMois { get; set; }
    public DateTime DateDebut { get; set; }
    public DateTime DateFin { get; set; }
    public NiveauEtude NiveauEtude { get; set; }
    public string Ecole { get; set; } = string.Empty;
    public string LettreMotivation { get; set; } = string.Empty;
    public string CvFileName { get; set; } = string.Empty;
    public string CvPath { get; set; } = string.Empty;
    public string Statut { get; set; } = string.Empty;
    public int? EncadrantId { get; set; }
    public string? Commentaire { get; set; }
    public DateTime DateCreation { get; set; }
    public DateTime? DateMiseAJour { get; set; }
    public DateTime? DateDecision { get; set; }
    public bool TransmisADirection { get; set; }
    public DateTime? DateTransmissionDirection { get; set; }
}

/// <summary>
/// ViewModel pour la création d'une candidature
/// </summary>
public class CreateCandidatureViewModel
{
    public int StagiaireId { get; set; }
    public int DepartementId { get; set; }
    public int SpecialiteId { get; set; }
    public int DureeStageMois { get; set; }
    public DateTime DateDebut { get; set; }
    public DateTime DateFin { get; set; }
    public NiveauEtude NiveauEtude { get; set; }
    public string Ecole { get; set; } = string.Empty;
    public string LettreMotivation { get; set; } = string.Empty;
    public string CvFileName { get; set; } = string.Empty;
    public string CvPath { get; set; } = string.Empty;
}

/// <summary>
/// ViewModel pour le suivi d'une candidature
/// </summary>
public class CandidatureSuiviViewModel
{
    public Guid Id { get; set; }
    public string Statut { get; set; } = string.Empty;
    public DateTime DateCreation { get; set; }
    public DateTime? DateDecision { get; set; }
    public string? Commentaire { get; set; }

    public string StatutBadgeClass => Statut switch
    {
        "EnAttente" => "bg-warning",
        "Acceptee" => "bg-success",
        "Refusee" => "bg-danger",
        _ => "bg-secondary"
    };

    public string StatutLabel => Statut switch
    {
        "EnAttente" => "En attente",
        "Acceptee" => "Acceptée",
        "Refusee" => "Refusée",
        _ => Statut
    };
}
