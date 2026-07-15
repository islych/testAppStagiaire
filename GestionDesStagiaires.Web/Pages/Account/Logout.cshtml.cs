using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GestionDesStagiaires.Web.Services;

namespace GestionDesStagiaires.Web.Pages.Account;

public class LogoutModel : PageModel
{
    private readonly IAuthenticationApiService _authService;
    private readonly ILogger<LogoutModel> _logger;

    public LogoutModel(
        IAuthenticationApiService authService,
        ILogger<LogoutModel> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            _logger.LogInformation("Déconnexion de {User}", User.Identity?.Name);

            // Appel au service de déconnexion (optionnel)
            await _authService.LogoutAsync();

            // Déconnexion des cookies
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Nettoyer la session
            HttpContext.Session.Clear();

            _logger.LogInformation("Déconnexion réussie");

            return RedirectToPage("/index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la déconnexion");
            return RedirectToPage("/index");
        }
    }
}
