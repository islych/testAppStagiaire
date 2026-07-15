using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using GestionDesStagiaires.Web.Models;
using GestionDesStagiaires.Web.Services;

namespace GestionDesStagiaires.Web.Pages.Admin;

[Authorize(Roles = "Administrateur")]
public class IndexModel : PageModel
{
    private readonly IUserManagementService _userService;
    private readonly ILogger<IndexModel> _logger;

    public List<UserDto> Users { get; set; } = new();
    public List<RoleDto> Roles { get; set; } = new();

    [BindProperty]
    public CreateUserRequest NewUser { get; set; } = new();

    public string? SuccessMessage { get; set; }
    public string? ErrorMessage { get; set; }

    public IndexModel(IUserManagementService userService, ILogger<IndexModel> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        _logger.LogInformation("Admin page consultée par {User}", User.Identity?.Name);
        
        var token = HttpContext.Session.GetString("JwtToken");
        if (string.IsNullOrEmpty(token))
        {
            return RedirectToPage("/Account/Login");
        }

        await LoadDataAsync(token);
        return Page();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        var token = HttpContext.Session.GetString("JwtToken");
        if (string.IsNullOrEmpty(token))
        {
            return RedirectToPage("/Account/Login");
        }

        if (!ModelState.IsValid)
        {
            await LoadDataAsync(token);
            return Page();
        }

        var result = await _userService.CreateUserAsync(NewUser, token);
        
        if (result.Success)
        {
            SuccessMessage = $"Utilisateur {NewUser.Email} créé avec succès. Mot de passe temporaire : {result.Message}";
            NewUser = new CreateUserRequest();
        }
        else
        {
            ErrorMessage = result.Message ?? "Erreur lors de la création de l'utilisateur";
        }

        await LoadDataAsync(token);
        return Page();
    }

    public async Task<IActionResult> OnPostToggleStatusAsync(int userId)
    {
        var token = HttpContext.Session.GetString("JwtToken");
        if (string.IsNullOrEmpty(token))
        {
            return RedirectToPage("/Account/Login");
        }

        var result = await _userService.ToggleUserStatusAsync(userId, token);
        
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message ?? "Erreur lors du basculement du statut";
        }

        await LoadDataAsync(token);
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int userId)
    {
        var token = HttpContext.Session.GetString("JwtToken");
        if (string.IsNullOrEmpty(token))
        {
            return RedirectToPage("/Account/Login");
        }

        var result = await _userService.DeleteUserAsync(userId, token);
        
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.Message ?? "Erreur lors de la suppression";
        }

        await LoadDataAsync(token);
        return Page();
    }

    private async Task LoadDataAsync(string token)
    {
        Users = await _userService.GetAllUsersAsync(token);
        Roles = await _userService.GetRolesAsync(token);
    }
}
