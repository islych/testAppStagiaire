using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GestionDesStagiaires.Web.Pages.Notifications;

[Authorize(Roles = "Stagiaire")]
public class IndexModel : PageModel
{
    private readonly IConfiguration _configuration;

    public string NotificationsApiUrl { get; set; } = string.Empty;
    public string JwtToken { get; set; } = string.Empty;

    public IndexModel(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void OnGet()
    {
        NotificationsApiUrl = _configuration["ApiUrls:NotificationsApi"] ?? "http://localhost:5132";
        JwtToken = HttpContext.Session.GetString("JwtToken") ?? string.Empty;
    }
}
