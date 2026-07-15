namespace Candidatures.Domain.Enums;

/// <summary>
/// Énumération des départements métier de l'ONEE-BE
/// Cette est l'unique source de vérité pour les départements.
/// </summary>
public enum Departement
{
    /// <summary>Ressources Humaines (ID: 1)</summary>
    [DepartementDisplay("Ressources Humaines", "Gestion des ressources humaines et recrutement")]
    RessourcesHumaines = 1,

    /// <summary>Systèmes d'Information (ID: 2)</summary>
    [DepartementDisplay("Systèmes d'Information", "Infrastructure IT et systèmes d'information")]
    SystemesInformation = 2,

    /// <summary>Développement Informatique (ID: 3)</summary>
    [DepartementDisplay("Développement Informatique", "Développement logiciels et applications")]
    DeveloppementInformatique = 3,

    /// <summary>Réseaux et Télécommunications (ID: 4)</summary>
    [DepartementDisplay("Réseaux et Télécommunications", "Infrastructure réseau et télécommunications")]
    ReseauxTelecommunications = 4,

    /// <summary>Électrotechnique (ID: 5)</summary>
    [DepartementDisplay("Électrotechnique", "Électrotechnique générale")]
    Electrotechnique = 5,

    /// <summary>Électromécanique (ID: 6)</summary>
    [DepartementDisplay("Électromécanique", "Électromécanique générale")]
    Electromecanique = 6,

    /// <summary>Maintenance Industrielle (ID: 7)</summary>
    [DepartementDisplay("Maintenance Industrielle", "Maintenance des équipements industriels")]
    MaintenanceIndustrielle = 7,

    /// <summary>Production (ID: 8)</summary>
    [DepartementDisplay("Production", "Production générale")]
    Production = 8,

    /// <summary>Distribution (ID: 9)</summary>
    [DepartementDisplay("Distribution", "Distribution générale")]
    Distribution = 9,

    /// <summary>Transport d'Électricité (ID: 10)</summary>
    [DepartementDisplay("Transport d'Électricité", "Transport et transmission d'électricité")]
    TransportElectricite = 10,

    /// <summary>Énergies Renouvelables (ID: 11)</summary>
    [DepartementDisplay("Énergies Renouvelables", "Énergies renouvelables")]
    EnergiesRenouvelables = 11,

    /// <summary>Génie Civil (ID: 12)</summary>
    [DepartementDisplay("Génie Civil", "Génie civil général")]
    GenieCivil = 12,

    /// <summary>Achats et Logistique (ID: 13)</summary>
    [DepartementDisplay("Achats et Logistique", "Achats et logistique")]
    AchatsLogistique = 13,

    /// <summary>Finance et Comptabilité (ID: 14)</summary>
    [DepartementDisplay("Finance et Comptabilité", "Finance et gestion comptable")]
    FinanceComptabilite = 14,

    /// <summary>Audit et Contrôle de Gestion (ID: 15)</summary>
    [DepartementDisplay("Audit et Contrôle de Gestion", "Audit et contrôle de gestion")]
    AuditControleGestion = 15,

    /// <summary>Juridique (ID: 16)</summary>
    [DepartementDisplay("Juridique", "Affaires juridiques")]
    Juridique = 16,

    /// <summary>Communication (ID: 17)</summary>
    [DepartementDisplay("Communication", "Communication d'entreprise")]
    Communication = 17,

    /// <summary>Qualité, Sécurité et Environnement (ID: 18)</summary>
    [DepartementDisplay("Qualité, Sécurité et Environnement (QSE)", "Qualité, sécurité et environnement")]
    QSE = 18
}

/// <summary>
/// Attribut pour stocker le nom et la description affichée d'un département
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class DepartementDisplayAttribute : Attribute
{
    /// <summary>Nom affiché du département</summary>
    public string Nom { get; set; }

    /// <summary>Description du département</summary>
    public string Description { get; set; }

    /// <summary>
    /// Initialise l'attribut avec le nom et la description du département
    /// </summary>
    public DepartementDisplayAttribute(string nom, string description)
    {
        Nom = nom;
        Description = description;
    }
}
