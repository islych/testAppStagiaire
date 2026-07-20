namespace Candidatures.Application.Interfaces;

/// <summary>
/// Interface pour récupérer les infos utilisateur (email, nom) depuis Authentication.Service
/// Abstraction utilisée par la couche Application
/// </summary>
public interface IUserLookupService
{
    Task<(string Email, string NomComplet)?> GetUserInfoAsync(int userId, string token);
}
