namespace Candidatures.API.DTOs;

/// <summary>
/// DTO pour la réponse API d'un département
/// </summary>
public class DepartementDto
{
    /// <summary>ID unique du département</summary>
    public int Id { get; set; }

    /// <summary>Nom du département</summary>
    public string Nom { get; set; } = string.Empty;

    /// <summary>Description du département</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Nombre de spécialités disponibles dans ce département</summary>
    public int NombreSpecialites { get; set; }
}

/// <summary>
/// DTO pour la réponse API d'une spécialité
/// </summary>
public class SpecialiteDto
{
    /// <summary>ID unique de la spécialité</summary>
    public int Id { get; set; }

    /// <summary>Nom de la spécialité</summary>
    public string Nom { get; set; } = string.Empty;

    /// <summary>ID du département parent</summary>
    public int DepartementId { get; set; }
}

/// <summary>
/// DTO pour la réponse API liste complète des départements avec leurs spécialités
/// </summary>
public class DepartementDetailDto
{
    /// <summary>ID unique du département</summary>
    public int Id { get; set; }

    /// <summary>Nom du département</summary>
    public string Nom { get; set; } = string.Empty;

    /// <summary>Description du département</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Liste des spécialités du département</summary>
    public List<SpecialiteDto> Specialites { get; set; } = new();
}
