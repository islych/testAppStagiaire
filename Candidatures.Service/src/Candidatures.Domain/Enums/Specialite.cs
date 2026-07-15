namespace Candidatures.Domain.Enums;

/// <summary>
/// Énumération des spécialités métier par département
/// Cette est l'unique source de vérité pour les spécialités.
/// Les valeurs d'enum n'ont pas de sens métier - elles servent juste d'identifiants uniques.
/// L'association à un département est gérée par l'attribut SpecialiteDisplay.
/// </summary>
public enum Specialite
{
    // === Développement Informatique (Département 3) ===
    [SpecialiteDisplay("Développement .NET", Departement.DeveloppementInformatique)]
    DeveloppementDotNet = 101,

    [SpecialiteDisplay("Développement Java", Departement.DeveloppementInformatique)]
    DeveloppementJava = 102,

    [SpecialiteDisplay("Développement Web", Departement.DeveloppementInformatique)]
    DeveloppementWeb = 103,

    [SpecialiteDisplay("Développement Mobile", Departement.DeveloppementInformatique)]
    DeveloppementMobile = 104,

    [SpecialiteDisplay("DevOps", Departement.DeveloppementInformatique)]
    DevOps = 105,

    [SpecialiteDisplay("Intelligence Artificielle", Departement.DeveloppementInformatique)]
    IntelligenceArtificielle = 106,

    // === Systèmes d'Information (Département 2) ===
    [SpecialiteDisplay("Administration Systèmes", Departement.SystemesInformation)]
    AdministrationSystemes = 201,

    [SpecialiteDisplay("Cloud", Departement.SystemesInformation)]
    Cloud = 202,

    [SpecialiteDisplay("Cybersécurité", Departement.SystemesInformation)]
    Cybersecurite = 203,

    [SpecialiteDisplay("Base de Données", Departement.SystemesInformation)]
    BaseDonnees = 204,

    // === Réseaux et Télécommunications (Département 4) ===
    [SpecialiteDisplay("Administration Réseau", Departement.ReseauxTelecommunications)]
    AdministrationReseau = 301,

    [SpecialiteDisplay("Télécommunications", Departement.ReseauxTelecommunications)]
    Telecommunications = 302,

    [SpecialiteDisplay("Réseaux Industriels", Departement.ReseauxTelecommunications)]
    ReseauxIndustriels = 303,

    // === Finance (Département 14) ===
    [SpecialiteDisplay("Comptabilité", Departement.FinanceComptabilite)]
    Comptabilite = 401,

    [SpecialiteDisplay("Contrôle de Gestion", Departement.FinanceComptabilite)]
    ControleGestion = 402,

    [SpecialiteDisplay("Trésorerie", Departement.FinanceComptabilite)]
    Tresorerie = 403,

    // === Ressources Humaines (Département 1) ===
    [SpecialiteDisplay("Gestion RH", Departement.RessourcesHumaines)]
    GestionRH = 501,

    [SpecialiteDisplay("Recrutement", Departement.RessourcesHumaines)]
    Recrutement = 502,

    [SpecialiteDisplay("Formation", Departement.RessourcesHumaines)]
    Formation = 503,

    // === Électrotechnique (Département 5) ===
    [SpecialiteDisplay("Électrotechnique Générale", Departement.Electrotechnique)]
    ElectrotechniqueGenerale = 601,

    // === Électromécanique (Département 6) ===
    [SpecialiteDisplay("Électromécanique Générale", Departement.Electromecanique)]
    ElectromechaniqueGenerale = 701,

    // === Maintenance Industrielle (Département 7) ===
    [SpecialiteDisplay("Maintenance Générale", Departement.MaintenanceIndustrielle)]
    MaintenanceGenerale = 801,

    // === Production (Département 8) ===
    [SpecialiteDisplay("Production Générale", Departement.Production)]
    ProductionGenerale = 901,

    // === Distribution (Département 9) ===
    [SpecialiteDisplay("Distribution Générale", Departement.Distribution)]
    DistributionGenerale = 1001,

    // === Transport d'Électricité (Département 10) ===
    [SpecialiteDisplay("Transport Général", Departement.TransportElectricite)]
    TransportGeneral = 1101,

    // === Énergies Renouvelables (Département 11) ===
    [SpecialiteDisplay("Énergies Renouvelables", Departement.EnergiesRenouvelables)]
    EnergiesRenouvelablesSpec = 1201,

    // === Génie Civil (Département 12) ===
    [SpecialiteDisplay("Génie Civil Général", Departement.GenieCivil)]
    GenieCivilGeneral = 1301,

    // === Achats et Logistique (Département 13) ===
    [SpecialiteDisplay("Achats", Departement.AchatsLogistique)]
    Achats = 1401,

    [SpecialiteDisplay("Logistique", Departement.AchatsLogistique)]
    Logistique = 1402,

    // === Audit (Département 15) ===
    [SpecialiteDisplay("Audit", Departement.AuditControleGestion)]
    Audit = 1501,

    // === Juridique (Département 16) ===
    [SpecialiteDisplay("Droit du Travail", Departement.Juridique)]
    DroitTravail = 1601,

    // === Communication (Département 17) ===
    [SpecialiteDisplay("Communication Interne", Departement.Communication)]
    CommunicationInterne = 1701,

    // === Qualité, Sécurité et Environnement (Département 18) ===
    [SpecialiteDisplay("Qualité", Departement.QSE)]
    Qualite = 1801,

    [SpecialiteDisplay("Sécurité", Departement.QSE)]
    Securite = 1802,

    [SpecialiteDisplay("Environnement", Departement.QSE)]
    Environnement = 1803
}

/// <summary>
/// Attribut pour stocker le nom et le département d'une spécialité
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class SpecialiteDisplayAttribute : Attribute
{
    /// <summary>Nom affiché de la spécialité</summary>
    public string Nom { get; set; }

    /// <summary>Département parent</summary>
    public Departement DepartementId { get; set; }

    /// <summary>
    /// Initialise l'attribut avec le nom et le département de la spécialité
    /// </summary>
    public SpecialiteDisplayAttribute(string nom, Departement departementId)
    {
        Nom = nom;
        DepartementId = departementId;
    }
}
