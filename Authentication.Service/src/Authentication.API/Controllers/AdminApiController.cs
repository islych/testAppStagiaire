using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Authentication.Infrastructure.Persistence;
using Authentication.Domain.Entities;
using Authentication.API.Models;
using System.Security.Claims;

namespace Authentication.API.Controllers
{
    [ApiController]
    [Route("api/v1/admin")]
    [Authorize(Roles = "Administrateur")]
    public class AdminApiController : ControllerBase
    {
        private readonly AuthenticationDbContext _context;
        private readonly ILogger<AdminApiController> _logger;

        public AdminApiController(AuthenticationDbContext context, ILogger<AdminApiController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Récupère tous les utilisateurs
        /// </summary>
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var utilisateurs = await _context.Utilisateurs
                    .Include(u => u.Role)
                    .OrderByDescending(u => u.DateCreation)
                    .Select(u => new
                    {
                        u.Id,
                        u.Nom,
                        u.Prenom,
                        u.Email,
                        Role = u.Role!.Nom,
                        u.DepartementId,
                        u.Statut,
                        u.DateCreation
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = utilisateurs
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des utilisateurs");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Erreur lors de la récupération des utilisateurs"
                });
            }
        }

        /// <summary>
        /// Récupère tous les rôles
        /// </summary>
        [HttpGet("roles")]
        public async Task<IActionResult> GetRoles()
        {
            try
            {
                var roles = await _context.Roles
                    .Select(r => new { r.Id, r.Nom })
                    .ToListAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = roles
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des rôles");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Erreur lors de la récupération des rôles"
                });
            }
        }

        /// <summary>
        /// Crée un nouvel utilisateur
        /// </summary>
        [HttpPost("create-user")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            try
            {
                // Valider la requête
                if (string.IsNullOrWhiteSpace(request.Nom) || string.IsNullOrWhiteSpace(request.Prenom) 
                    || string.IsNullOrWhiteSpace(request.Email))
                {
                    return BadRequest(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Nom, Prénom et Email sont obligatoires"
                    });
                }

                // Vérifier si l'email existe déjà
                if (await _context.Utilisateurs.AnyAsync(u => u.Email == request.Email))
                {
                    return BadRequest(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Cet email est déjà utilisé"
                    });
                }

                // Vérifier que le rôle existe
                var role = await _context.Roles.FindAsync(request.RoleId);
                if (role == null)
                {
                    return BadRequest(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Rôle invalide"
                    });
                }

                // Générer un mot de passe temporaire
                var motDePasseTemp = GenerateTemporaryPassword();

                // Créer l'utilisateur
                var utilisateur = new Utilisateur
                {
                    Nom = request.Nom,
                    Prenom = request.Prenom,
                    Email = request.Email,
                    MotDePasseHash = BCrypt.Net.BCrypt.HashPassword(motDePasseTemp),
                    RoleId = request.RoleId,
                    DepartementId = role.Nom == "Encadrant" ? request.DepartementId : null,
                    Statut = true,
                    DateCreation = DateTime.Now
                };

                _context.Utilisateurs.Add(utilisateur);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Nouvel utilisateur créé : {Email}", request.Email);

                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Message = motDePasseTemp,
                    Data = utilisateur.Id.ToString()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création de l'utilisateur");
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = "Erreur lors de la création de l'utilisateur"
                });
            }
        }

        /// <summary>
        /// Bascule le statut d'un utilisateur
        /// </summary>
        [HttpPost("toggle-status/{userId}")]
        public async Task<IActionResult> ToggleStatus(int userId)
        {
            try
            {
                var utilisateur = await _context.Utilisateurs
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (utilisateur == null)
                {
                    return NotFound(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Utilisateur non trouvé"
                    });
                }

                // Empêcher la désactivation du dernier admin actif
                if (utilisateur.Role!.Nom == "Administrateur" && utilisateur.Statut)
                {
                    var nbAdminsActifs = await _context.Utilisateurs
                        .Include(u => u.Role)
                        .CountAsync(u => u.Role!.Nom == "Administrateur" && u.Statut);

                    if (nbAdminsActifs <= 1)
                    {
                        return BadRequest(new ApiResponse<string>
                        {
                            Success = false,
                            Message = "Impossible de désactiver le dernier administrateur actif"
                        });
                    }
                }

                utilisateur.Statut = !utilisateur.Statut;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Statut de l'utilisateur {Email} basculé", utilisateur.Email);

                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Message = "Statut basculé avec succès"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du basculement du statut");
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = "Erreur lors du basculement du statut"
                });
            }
        }

        /// <summary>
        /// Supprime un utilisateur
        /// </summary>
        [HttpDelete("delete-user/{userId}")]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            try
            {
                var utilisateur = await _context.Utilisateurs
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (utilisateur == null)
                {
                    return NotFound(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Utilisateur non trouvé"
                    });
                }

                // Empêcher la suppression du dernier admin
                if (utilisateur.Role!.Nom == "Administrateur")
                {
                    var nbAdmins = await _context.Utilisateurs
                        .Include(u => u.Role)
                        .CountAsync(u => u.Role!.Nom == "Administrateur");

                    if (nbAdmins <= 1)
                    {
                        return BadRequest(new ApiResponse<string>
                        {
                            Success = false,
                            Message = "Impossible de supprimer le dernier administrateur"
                        });
                    }
                }

                _context.Utilisateurs.Remove(utilisateur);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Utilisateur {Email} supprimé", utilisateur.Email);

                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Message = "Utilisateur supprimé avec succès"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression de l'utilisateur");
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = "Erreur lors de la suppression de l'utilisateur"
                });
            }
        }

        private string GenerateTemporaryPassword()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%";
            var random = new Random();
            var password = new string(Enumerable.Repeat(chars, 10)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            return password;
        }
    }

    public record CreateUserRequest(string Nom, string Prenom, string Email, int RoleId, int? DepartementId = null);
}
