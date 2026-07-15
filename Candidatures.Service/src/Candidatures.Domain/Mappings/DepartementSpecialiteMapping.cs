using Candidatures.Domain.Enums;
using System.Reflection;

namespace Candidatures.Domain.Mappings;

/// <summary>
/// Classe utilitaire pour gérer les mappages entre départements et spécialités
/// Source unique de vérité pour les métadonnées des énums métier
/// </summary>
public static class DepartementSpecialiteMapping
{
    /// <summary>
    /// Récupère tous les départements avec leurs métadonnées
    /// </summary>
    public static List<DepartementInfo> GetAllDepartements()
    {
        var departements = new List<DepartementInfo>();

        foreach (Departement dept in Enum.GetValues(typeof(Departement)))
        {
            var fieldInfo = typeof(Departement).GetField(dept.ToString());
            var attribute = fieldInfo?.GetCustomAttribute<DepartementDisplayAttribute>();

            if (attribute != null)
            {
                departements.Add(new DepartementInfo
                {
                    Id = (int)dept,
                    Code = dept.ToString(),
                    Nom = attribute.Nom,
                    Description = attribute.Description
                });
            }
        }

        return departements.OrderBy(d => d.Id).ToList();
    }

    /// <summary>
    /// Récupère un département par son ID
    /// </summary>
    public static DepartementInfo? GetDepartementById(int departementId)
    {
        return GetAllDepartements().FirstOrDefault(d => d.Id == departementId);
    }

    /// <summary>
    /// Vérifie si un ID correspond à un département valide
    /// </summary>
    public static bool IsDepartementValide(int departementId)
    {
        return GetAllDepartements().Any(d => d.Id == departementId);
    }

    /// <summary>
    /// Récupère toutes les spécialités pour un département donné
    /// </summary>
    public static List<SpecialiteInfo> GetSpecialitesByDepartement(int departementId)
    {
        if (!IsDepartementValide(departementId))
            return new List<SpecialiteInfo>();

        var departement = (Departement)departementId;
        var specialites = new List<SpecialiteInfo>();

        foreach (Specialite spec in Enum.GetValues(typeof(Specialite)))
        {
            var fieldInfo = typeof(Specialite).GetField(spec.ToString());
            var attribute = fieldInfo?.GetCustomAttribute<SpecialiteDisplayAttribute>();

            if (attribute != null && (int)attribute.DepartementId == departementId)
            {
                specialites.Add(new SpecialiteInfo
                {
                    Id = (int)spec,
                    Code = spec.ToString(),
                    Nom = attribute.Nom,
                    DepartementId = departementId
                });
            }
        }

        return specialites.OrderBy(s => s.Id).ToList();
    }

    /// <summary>
    /// Récupère toutes les spécialités
    /// </summary>
    public static List<SpecialiteInfo> GetAllSpecialites()
    {
        var specialites = new List<SpecialiteInfo>();

        foreach (Specialite spec in Enum.GetValues(typeof(Specialite)))
        {
            var fieldInfo = typeof(Specialite).GetField(spec.ToString());
            var attribute = fieldInfo?.GetCustomAttribute<SpecialiteDisplayAttribute>();

            if (attribute != null)
            {
                specialites.Add(new SpecialiteInfo
                {
                    Id = (int)spec,
                    Code = spec.ToString(),
                    Nom = attribute.Nom,
                    DepartementId = (int)attribute.DepartementId
                });
            }
        }

        return specialites.OrderBy(s => s.Id).ToList();
    }

    /// <summary>
    /// Récupère une spécialité par son ID
    /// </summary>
    public static SpecialiteInfo? GetSpecialiteById(int specialiteId)
    {
        return GetAllSpecialites().FirstOrDefault(s => s.Id == specialiteId);
    }

    /// <summary>
    /// Vérifie si une spécialité appartient à un département
    /// </summary>
    public static bool IsSpecialiteValideForDepartement(int departementId, int specialiteId)
    {
        return GetSpecialitesByDepartement(departementId).Any(s => s.Id == specialiteId);
    }
}

/// <summary>
/// DTO pour représenter les informations d'un département
/// </summary>
public class DepartementInfo
{
    /// <summary>ID unique du département</summary>
    public int Id { get; set; }

    /// <summary>Code (nom d'enum) du département</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Nom affiché du département</summary>
    public string Nom { get; set; } = string.Empty;

    /// <summary>Description du département</summary>
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// DTO pour représenter les informations d'une spécialité
/// </summary>
public class SpecialiteInfo
{
    /// <summary>ID unique de la spécialité</summary>
    public int Id { get; set; }

    /// <summary>Code (nom d'enum) de la spécialité</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Nom affiché de la spécialité</summary>
    public string Nom { get; set; } = string.Empty;

    /// <summary>ID du département parent</summary>
    public int DepartementId { get; set; }
}
