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
    [Route("api/v1/users")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly AuthenticationDbContext _context;
        private readonly ILogger<UsersController> _logger;

        public UsersController(AuthenticationDbContext context, ILogger<UsersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Récupère un utilisateur par son ID
        /// </summary>
        /// <remarks>
        /// Permet aux autres microservices de récupérer les informations d'un utilisateur spécifique
        /// </remarks>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUserById(int id)
        {
            try
            {
                _logger.LogInformation("Récupération de l'utilisateur {UserId}", id);

                var user = await _context.Utilisateurs
                    .Include(u => u.Role)
                    .Where(u => u.Id == id && u.Statut)
                    .Select(u => new UserDto
                    {
                        Id = u.Id,
                        Nom = u.Nom,
                        Prenom = u.Prenom,
                        Email = u.Email,
                        Role = u.Role!.Nom,
                        Statut = u.Statut,
                        DateCreation = u.DateCreation
                    })
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    _logger.LogWarning("Utilisateur {UserId} non trouvé ou désactivé", id);
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Utilisateur non trouvé ou désactivé"
                    });
                }

                return Ok(new ApiResponse<UserDto>
                {
                    Success = true,
                    Message = "Utilisateur trouvé",
                    Data = user
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de l'utilisateur {UserId}", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Erreur serveur lors de la récupération de l'utilisateur"
                });
            }
        }

        /// <summary>
        /// Récupère tous les stagiaires
        /// </summary>
        /// <remarks>
        /// Permet aux autres microservices de récupérer la liste de tous les stagiaires
        /// </remarks>
        [HttpGet("stagiaires")]
        [Authorize(Roles = "Encadrant,Direction,RH,Administrateur")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<UserDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetStagiaires()
        {
            try
            {
                _logger.LogInformation("Récupération de tous les stagiaires");

                var stagiaires = await _context.Utilisateurs
                    .Include(u => u.Role)
                    .Where(u => u.Role!.Nom == "Stagiaire" && u.Statut)
                    .OrderBy(u => u.Nom)
                    .ThenBy(u => u.Prenom)
                    .Select(u => new UserDto
                    {
                        Id = u.Id,
                        Nom = u.Nom,
                        Prenom = u.Prenom,
                        Email = u.Email,
                        Role = u.Role!.Nom,
                        Statut = u.Statut,
                        DateCreation = u.DateCreation
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<IEnumerable<UserDto>>
                {
                    Success = true,
                    Message = $"{stagiaires.Count} stagiaire(s) trouvé(s)",
                    Data = stagiaires
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des stagiaires");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Erreur serveur lors de la récupération des stagiaires"
                });
            }
        }

        /// <summary>
        /// Récupère tous les encadrants
        /// </summary>
        /// <remarks>
        /// Permet aux autres microservices de récupérer la liste de tous les encadrants
        /// </remarks>
        [HttpGet("encadrants")]
        [Authorize(Roles = "Stagiaire,Direction,RH,Administrateur")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<UserDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetEncadrants()
        {
            try
            {
                _logger.LogInformation("Récupération de tous les encadrants");

                var encadrants = await _context.Utilisateurs
                    .Include(u => u.Role)
                    .Where(u => u.Role!.Nom == "Encadrant" && u.Statut)
                    .OrderBy(u => u.Nom)
                    .ThenBy(u => u.Prenom)
                    .Select(u => new UserDto
                    {
                        Id = u.Id,
                        Nom = u.Nom,
                        Prenom = u.Prenom,
                        Email = u.Email,
                        Role = u.Role!.Nom,
                        Statut = u.Statut,
                        DateCreation = u.DateCreation
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<IEnumerable<UserDto>>
                {
                    Success = true,
                    Message = $"{encadrants.Count} encadrant(s) trouvé(s)",
                    Data = encadrants
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des encadrants");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Erreur serveur lors de la récupération des encadrants"
                });
            }
        }

        /// <summary>
        /// Récupère le profil de l'utilisateur connecté
        /// </summary>
        [HttpGet("me/profile")]
        [ProducesResponseType(typeof(ApiResponse<UserProfileDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMyProfile()
        {
            try
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdStr, out var userId))
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Identifiant utilisateur invalide"
                    });
                }

                _logger.LogInformation("Récupération du profil utilisateur {UserId}", userId);

                var user = await _context.Utilisateurs
                    .Include(u => u.Role)
                    .Where(u => u.Id == userId && u.Statut)
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Utilisateur non trouvé ou désactivé"
                    });
                }

                var profile = new UserProfileDto
                {
                    Id = user.Id,
                    Nom = user.Nom,
                    Prenom = user.Prenom,
                    Email = user.Email,
                    Role = user.Role!.Nom,
                    Statut = user.Statut,
                    DateCreation = user.DateCreation
                };

                return Ok(new ApiResponse<UserProfileDto>
                {
                    Success = true,
                    Message = "Profil récupéré avec succès",
                    Data = profile
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération du profil utilisateur");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Erreur serveur lors de la récupération du profil"
                });
            }
        }

        /// <summary>
        /// Recherche des utilisateurs par nom ou prénom
        /// </summary>
        /// <remarks>
        /// Permet aux autres microservices de rechercher des utilisateurs
        /// </remarks>
        [HttpGet("search")]
        [Authorize(Roles = "Encadrant,Direction,RH,Administrateur")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<UserDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SearchUsers([FromQuery] string query, [FromQuery] string? role = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Le paramètre de recherche est requis"
                    });
                }

                _logger.LogInformation("Recherche d'utilisateurs avec query: {Query}, role: {Role}", query, role ?? "tous");

                var searchQuery = _context.Utilisateurs
                    .Include(u => u.Role)
                    .Where(u => u.Statut);

                if (!string.IsNullOrEmpty(role))
                {
                    searchQuery = searchQuery.Where(u => u.Role!.Nom == role);
                }

                var users = await searchQuery
                    .Where(u => u.Nom.Contains(query) || u.Prenom.Contains(query) || u.Email.Contains(query))
                    .OrderBy(u => u.Nom)
                    .ThenBy(u => u.Prenom)
                    .Select(u => new UserDto
                    {
                        Id = u.Id,
                        Nom = u.Nom,
                        Prenom = u.Prenom,
                        Email = u.Email,
                        Role = u.Role!.Nom,
                        Statut = u.Statut,
                        DateCreation = u.DateCreation
                    })
                    .Take(50) // Limite pour éviter des résultats trop volumineux
                    .ToListAsync();

                return Ok(new ApiResponse<IEnumerable<UserDto>>
                {
                    Success = true,
                    Message = $"{users.Count} utilisateur(s) trouvé(s)",
                    Data = users
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la recherche d'utilisateurs");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Erreur serveur lors de la recherche"
                });
            }
        }

        /// <summary>
        /// Récupère plusieurs utilisateurs par leurs IDs
        /// </summary>
        /// <remarks>
        /// Permet aux autres microservices de récupérer plusieurs utilisateurs en une seule requête
        /// </remarks>
        [HttpPost("batch")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<UserDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUsersBatch([FromBody] List<int> userIds)
        {
            try
            {
                if (userIds == null || userIds.Count == 0)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "La liste des IDs est requise"
                    });
                }

                if (userIds.Count > 100)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "La liste ne peut pas contenir plus de 100 IDs"
                    });
                }

                _logger.LogInformation("Récupération batch de {Count} utilisateurs", userIds.Count);

                var users = await _context.Utilisateurs
                    .Include(u => u.Role)
                    .Where(u => userIds.Contains(u.Id) && u.Statut)
                    .OrderBy(u => u.Nom)
                    .ThenBy(u => u.Prenom)
                    .Select(u => new UserDto
                    {
                        Id = u.Id,
                        Nom = u.Nom,
                        Prenom = u.Prenom,
                        Email = u.Email,
                        Role = u.Role!.Nom,
                        Statut = u.Statut,
                        DateCreation = u.DateCreation
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<IEnumerable<UserDto>>
                {
                    Success = true,
                    Message = $"{users.Count} utilisateur(s) trouvé(s)",
                    Data = users
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération batch d'utilisateurs");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Erreur serveur lors de la récupération batch"
                });
            }
        }
    }

    /// <summary>
    /// DTO pour les informations utilisateur
    /// </summary>
    public class UserDto
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool Statut { get; set; }
        public DateTime DateCreation { get; set; }
    }

    /// <summary>
    /// DTO pour le profil utilisateur
    /// </summary>
    public class UserProfileDto : UserDto
    {
        // Peut être étendu avec des informations spécifiques au profil
    }
}