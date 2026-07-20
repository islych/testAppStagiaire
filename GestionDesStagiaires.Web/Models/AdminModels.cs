namespace GestionDesStagiaires.Web.Models;

/// <summary>
/// DTO pour afficher un utilisateur
/// </summary>
public class UserDto
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int? DepartementId { get; set; }
    public bool Statut { get; set; }
    public DateTime DateCreation { get; set; }
}

/// <summary>
/// DTO pour un rôle
/// </summary>
public class RoleDto
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
}

/// <summary>
/// Requête de création d'utilisateur
/// </summary>
public class CreateUserRequest
{
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public int? DepartementId { get; set; }
}
