namespace Candidatures.Domain.Enums;

/// <summary>
/// Durée de stage disponible (en mois)
/// L'utilisateur choisit la durée, puis indique les dates de début et fin
/// </summary>
public enum DureeStageMois
{
    /// <summary>1 mois de stage</summary>
    [DureeStageDisplay("1 mois", 1)]
    UnMois = 1,
    
    /// <summary>2 mois de stage</summary>
    [DureeStageDisplay("2 mois", 2)]
    DeuxMois = 2,
    
    /// <summary>3 mois de stage</summary>
    [DureeStageDisplay("3 mois", 3)]
    TroisMois = 3,
    
    /// <summary>4 mois de stage</summary>
    [DureeStageDisplay("4 mois", 4)]
    QuatreMois = 4,
    
    /// <summary>5 mois de stage</summary>
    [DureeStageDisplay("5 mois", 5)]
    CinqMois = 5,
    
    /// <summary>6 mois de stage</summary>
    [DureeStageDisplay("6 mois", 6)]
    SixMois = 6
}

/// <summary>
/// Attribut pour stocker le nom affiché et la durée en mois
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class DureeStageDisplayAttribute : Attribute
{
    /// <summary>Nom affiché de la durée</summary>
    public string Nom { get; set; }

    /// <summary>Durée en mois</summary>
    public int Mois { get; set; }

    /// <summary>
    /// Initialise l'attribut avec le nom et la durée
    /// </summary>
    public DureeStageDisplayAttribute(string nom, int mois)
    {
        Nom = nom;
        Mois = mois;
    }
}
