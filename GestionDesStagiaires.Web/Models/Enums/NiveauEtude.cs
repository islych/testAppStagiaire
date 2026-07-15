namespace GestionDesStagiaires.Web.Models.Enums;

/// <summary>
/// Niveaux d'études (simplifié selon les normes de gestion des stages)
/// </summary>
public enum NiveauEtude
{
    /// <summary>Bac - Niveau secondaire</summary>
    Bac = 1,
    
    /// <summary>Bac +2 - BTS, DUT, etc.</summary>
    BacPlus2 = 2,
    
    /// <summary>Bac +3 - Licence professionnelle, Licence</summary>
    BacPlus3 = 3,
    
    /// <summary>Bac +4 - Maîtrise, Master 1</summary>
    BacPlus4 = 4,
    
    /// <summary>Ingénieur - Diplôme d'ingénieur</summary>
    Ingenieur = 5,
    
    /// <summary>Master - Master 2</summary>
    Master = 6,
    
    /// <summary>Doctorat - PhD</summary>
    Doctorat = 7,
    
    /// <summary>Autre - Autres formations</summary>
    Autre = 8
}
