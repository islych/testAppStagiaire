using Microsoft.AspNetCore.Mvc;

namespace Candidatures.API.Controllers;

/// <summary>
/// Contrôleur d'accueil
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class HomeController : ControllerBase
{
    /// <summary>
    /// Vérifie que le service est accessible
    /// </summary>
    /// <returns>Message de confirmation</returns>
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new
        {
            status = "healthy",
            service = "Candidatures.API",
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Récupère les informations du service
    /// </summary>
    /// <returns>Informations du service</returns>
    [HttpGet("info")]
    public IActionResult Info()
    {
        return Ok(new
        {
            serviceName = "Candidatures Service",
            version = "1.0.0",
            description = "Service de gestion des candidatures",
            endpoints = new
            {
                create = "POST /api/v1/candidatures",
                getAll = "GET /api/v1/candidatures",
                getById = "GET /api/v1/candidatures/{id}",
                getMy = "GET /api/v1/candidatures/me/candidatures",
                getSuivi = "GET /api/v1/candidatures/{id}/suivi",
                accept = "POST /api/v1/candidatures/{id}/accept",
                reject = "POST /api/v1/candidatures/{id}/reject"
            }
        });
    }
}
