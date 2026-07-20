using Candidatures.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Candidatures.Infrastructure.ExternalServices;

public class UserLookupService : IUserLookupService
{
    private readonly IAuthenticationServiceClient _authClient;
    private readonly ILogger<UserLookupService> _logger;

    public UserLookupService(IAuthenticationServiceClient authClient, ILogger<UserLookupService> logger)
    {
        _authClient = authClient;
        _logger = logger;
    }

    public async Task<(string Email, string NomComplet)?> GetUserInfoAsync(int userId, string token)
    {
        try
        {
            var user = await _authClient.GetUserByIdAsync(userId, token);
            if (user == null) return null;
            return (user.Email, $"{user.Prenom} {user.Nom}");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Impossible de récupérer les infos de l'utilisateur {UserId}", userId);
            return null;
        }
    }
}
