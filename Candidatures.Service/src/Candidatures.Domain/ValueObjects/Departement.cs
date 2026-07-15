namespace Candidatures.Domain.ValueObjects;

/// <summary>
/// Value Object représentant un département avec ses spécialités
/// </summary>
public class Departement
{
    /// <summary>
    /// Identifiant unique du département
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Nom du département
    /// </summary>
    public string Nom { get; set; } = string.Empty;

    /// <summary>
    /// Description du département
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Liste des spécialités pour ce département
    /// </summary>
    public List<Specialite> Specialites { get; set; } = new();

    /// <summary>
    /// Statut actif/inactif
    /// </summary>
    public bool Actif { get; set; } = true;
}

/// <summary>
/// Value Object représentant une spécialité
/// </summary>
public class Specialite
{
    /// <summary>
    /// Identifiant unique de la spécialité
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Nom de la spécialité
    /// </summary>
    public string Nom { get; set; } = string.Empty;

    /// <summary>
    /// Description de la spécialité
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Identifiant du département parent
    /// </summary>
    public int DepartementId { get; set; }

    /// <summary>
    /// Statut actif/inactif
    /// </summary>
    public bool Actif { get; set; } = true;
}
