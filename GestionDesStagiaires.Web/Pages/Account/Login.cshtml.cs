using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using GestionDesStagiaires.Web.Services;

namespace GestionDesStagiaires.Web.Pages.Account;

public class LoginModel : PageModel
{
    private readonly IAuthenticationApiService _authService;
    private readonly ILogger<LoginModel> _logger;

    [BindProperty]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    public string MotDePasse { get; set; } = string.Empty;

    [BindProperty]
    public bool RememberMe { get; set; }

    public string? ErrorMessage { get; set; }

    public LoginModel(
        IAuthenticationApiService authService,
        ILogger<LoginModel> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        // Si l'utilisateur est déjà connecté, rediriger vers le dashboard
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToPage("/Dashboard/Index");
        }

        // Nettoyer les cookies de session invalides
        try
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
        }
        catch { }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(MotDePasse))
        {
            ErrorMessage = "Email et mot de passe sont requis";
            return Page();
        }

        try
        {
            _logger.LogInformation("Tentative de connexion pour {Email}", Email);

            // Appel au service d'authentification
            var result = await _authService.LoginAsync(Email, MotDePasse);

            if (!result.Success || result.Data == null)
            {
                _logger.LogWarning("Échec de connexion pour {Email}", Email);
                ErrorMessage = result.Message ?? "Email ou mot de passe incorrect";
                return Page();
            }

            // Créer les claims de l'utilisateur
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, result.Data.Id.ToString()),
                new Claim(ClaimTypes.Email, result.Data.Email),
                new Claim(ClaimTypes.Name, $"{result.Data.Prenom} {result.Data.Nom}"),
                new Claim(ClaimTypes.GivenName, result.Data.Prenom),
                new Claim(ClaimTypes.Surname, result.Data.Nom),
                new Claim(ClaimTypes.Role, result.Data.Role),
                new Claim("JwtToken", result.Data.Token)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = RememberMe,
                ExpiresUtc = DateTime.UtcNow.AddDays(RememberMe ? 30 : 1)
            };

            // Authentifier l'utilisateur avec les cookies
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            // Stocker le JWT token dans la session pour les appels API
            HttpContext.Session.SetString("JwtToken", result.Data.Token);

            _logger.LogInformation("Connexion réussie pour {Email}", Email);
            TempData["StatusMessage"] = $"Bienvenue {result.Data.Prenom} !";

            return RedirectToPage("/Dashboard/Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la connexion");
            ErrorMessage = "Une erreur est survenue lors de la connexion";
            return Page();
        }
    }
}
