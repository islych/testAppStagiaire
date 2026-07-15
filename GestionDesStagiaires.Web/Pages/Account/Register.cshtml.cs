using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GestionDesStagiaires.Web.Models;
using GestionDesStagiaires.Web.Services;

namespace GestionDesStagiaires.Web.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly IRegistrationService _registrationService;
    private readonly ILogger<RegisterModel> _logger;

    [BindProperty]
    public RegisterRequest NewUser { get; set; } = new();

    public string? ErrorMessage { get; set; }
    public string? SuccessMessage { get; set; }

    public RegisterModel(IRegistrationService registrationService, ILogger<RegisterModel> logger)
    {
        _registrationService = registrationService;
        _logger = logger;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        // Si l'utilisateur est déjà connecté, rediriger
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToPage("/Dashboard/Index");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Si l'utilisateur est déjà connecté, rediriger
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToPage("/Dashboard/Index");
        }

        // Récupérer la valeur de la checkbox directement du formulaire
        var acceptTermsValue = Request.Form["acceptTerms"];
        bool acceptTerms = !string.IsNullOrEmpty(acceptTermsValue);

        // Valider les conditions d'utilisation
        if (!acceptTerms)
        {
            ErrorMessage = "Vous devez accepter les conditions d'utilisation";
            return Page();
        }

        // Vérifier ModelState
        if (!ModelState.IsValid)
        {
            ErrorMessage = "Veuillez corriger les erreurs et réessayer";
            return Page();
        }

        // Valider que les champs ne sont pas vides
        if (string.IsNullOrWhiteSpace(NewUser.Nom) || 
            string.IsNullOrWhiteSpace(NewUser.Prenom) || 
            string.IsNullOrWhiteSpace(NewUser.Email) ||
            string.IsNullOrWhiteSpace(NewUser.MotDePasse))
        {
            ErrorMessage = "Tous les champs sont obligatoires";
            return Page();
        }

        // Valider la longueur du mot de passe
        if (NewUser.MotDePasse.Length < 8)
        {
            ErrorMessage = "Le mot de passe doit contenir au minimum 8 caractères";
            return Page();
        }

        // Valider la confirmation du mot de passe
        if (NewUser.MotDePasse != NewUser.ConfirmMotDePasse)
        {
            ErrorMessage = "Les mots de passe ne correspondent pas";
            return Page();
        }

        // Valider le format de l'email
        if (!IsValidEmail(NewUser.Email))
        {
            ErrorMessage = "L'adresse email n'est pas valide";
            return Page();
        }

        try
        {
            _logger.LogInformation("Tentative d'enregistrement pour {Email}", NewUser.Email);

            // Appel au service d'enregistrement
            var result = await _registrationService.RegisterAsync(NewUser);

            if (!result.Success)
            {
                _logger.LogWarning("Erreur d'enregistrement pour {Email} : {Message}", NewUser.Email, result.Message);
                ErrorMessage = result.Message ?? "Erreur lors de l'enregistrement";
                return Page();
            }

            _logger.LogInformation("Enregistrement réussi pour {Email}", NewUser.Email);
            SuccessMessage = "Compte créé avec succès! Redirection vers la connexion...";
            
            // Réinitialiser le formulaire
            NewUser = new RegisterRequest();
            
            // Redirection après 2 secondes
            TempData["RegisterSuccess"] = true;
            TempData["SuccessEmail"] = NewUser.Email;
            
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'enregistrement");
            ErrorMessage = "Une erreur est survenue lors de l'enregistrement";
            return Page();
        }
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
