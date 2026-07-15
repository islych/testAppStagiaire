using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Authentication.Infrastructure.Persistence;
using Authentication.Domain.Entities;
using Authentication.Infrastructure.Security.JwtTokenGenerator;
using Authentication.API.Models;

namespace Authentication.API.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthApiController : ControllerBase
    {
        private readonly AuthenticationDbContext _context;
        private readonly JwtTokenGenerator _jwt;
        private readonly IConfiguration _config;
        private readonly ILogger<AuthApiController> _logger;

        public AuthApiController(AuthenticationDbContext context, JwtTokenGenerator jwt, IConfiguration config, ILogger<AuthApiController> logger)
        {
            _context = context;
            _jwt = jwt;
            _config = config;
            _logger = logger;
        }

        // POST /api/v1/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            var utilisateur = await _context.Utilisateurs
                .Include(u => u.Role)
                    .ThenInclude(r => r!.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(u => u.Email == req.Email);

            if (utilisateur == null || !BCrypt.Net.BCrypt.Verify(req.MotDePasse, utilisateur.MotDePasseHash))
                return Unauthorized(new ApiResponse<LoginResponse>
                {
                    Success = false,
                    Message = "Identifiants invalides."
                });

            if (!utilisateur.Statut)
                return StatusCode(403, new ApiResponse<LoginResponse>
                {
                    Success = false,
                    Message = "Compte désactivé."
                });

            var permissions = utilisateur.Role!.RolePermissions
                .Select(rp => rp.Permission!.Code)
                .ToList();

            var accessToken = _jwt.GenererAccessToken(utilisateur, permissions);
            var refreshTokenValeur = _jwt.GenererRefreshToken();

            var dureeRefresh = int.Parse(_config["Jwt:RefreshDureeJours"] ?? "7");
            var refreshToken = new RefreshToken
            {
                UtilisateurId = utilisateur.Id,
                Token = refreshTokenValeur,
                DateExpiration = DateTime.UtcNow.AddDays(dureeRefresh),
                AdresseIp = HttpContext.Connection.RemoteIpAddress?.ToString()
            };

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            var loginResponse = new LoginResponse
            {
                Id = utilisateur.Id,
                Email = utilisateur.Email,
                Nom = utilisateur.Nom,
                Prenom = utilisateur.Prenom,
                Role = utilisateur.Role.Nom,
                Token = accessToken
            };

            return Ok(new ApiResponse<LoginResponse>
            {
                Success = true,
                Message = "Connexion réussie",
                Data = loginResponse
            });
        }

        // POST /api/v1/auth/register
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req)
        {
            try
            {
                // Valider la requête
                if (string.IsNullOrWhiteSpace(req.Nom) || string.IsNullOrWhiteSpace(req.Prenom) 
                    || string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.MotDePasse))
                {
                    return BadRequest(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Tous les champs sont obligatoires"
                    });
                }

                // Valider la longueur du mot de passe
                if (req.MotDePasse.Length < 8)
                {
                    return BadRequest(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Le mot de passe doit contenir au minimum 8 caractères"
                    });
                }

                // Vérifier que les mots de passe correspondent
                if (req.MotDePasse != req.ConfirmMotDePasse)
                {
                    return BadRequest(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Les mots de passe ne correspondent pas"
                    });
                }

                // Vérifier si l'email existe déjà
                if (await _context.Utilisateurs.AnyAsync(u => u.Email == req.Email))
                {
                    return BadRequest(new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Cet email est déjà utilisé"
                    });
                }

                // Récupérer le rôle Stagiaire
                var roleStagiaire = await _context.Roles.FirstOrDefaultAsync(r => r.Nom == "Stagiaire");
                if (roleStagiaire == null)
                {
                    return StatusCode(500, new ApiResponse<string>
                    {
                        Success = false,
                        Message = "Erreur de configuration système"
                    });
                }

                // Créer le nouvel utilisateur
                var nouvelUtilisateur = new Utilisateur
                {
                    Nom = req.Nom,
                    Prenom = req.Prenom,
                    Email = req.Email,
                    MotDePasseHash = BCrypt.Net.BCrypt.HashPassword(req.MotDePasse),
                    RoleId = roleStagiaire.Id,
                    Statut = true,
                    DateCreation = DateTime.Now
                };

                _context.Utilisateurs.Add(nouvelUtilisateur);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Nouvel utilisateur enregistré : {Email}", req.Email);

                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Message = "Enregistrement réussi. Vous pouvez maintenant vous connecter.",
                    Data = nouvelUtilisateur.Id.ToString()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'enregistrement");
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = "Erreur lors de l'enregistrement"
                });
            }
        }

        // POST /api/v1/auth/refresh-token
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest req)
        {
            var token = await _context.RefreshTokens
                .Include(rt => rt.Utilisateur)
                    .ThenInclude(u => u!.Role)
                        .ThenInclude(r => r!.RolePermissions)
                            .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(rt => rt.Token == req.RefreshToken && !rt.Revoque);

            if (token == null || token.DateExpiration < DateTime.UtcNow)
                return Unauthorized(new { success = false, message = "Refresh token invalide ou expiré." });

            // Révoquer l'ancien
            token.Revoque = true;

            var permissions = token.Utilisateur!.Role!.RolePermissions
                .Select(rp => rp.Permission!.Code)
                .ToList();

            var nouveauAccess = _jwt.GenererAccessToken(token.Utilisateur, permissions);
            var nouveauRefresh = _jwt.GenererRefreshToken();

            var dureeRefresh = int.Parse(_config["Jwt:RefreshDureeJours"] ?? "7");
            _context.RefreshTokens.Add(new RefreshToken
            {
                UtilisateurId = token.UtilisateurId,
                Token = nouveauRefresh,
                DateExpiration = DateTime.UtcNow.AddDays(dureeRefresh),
                AdresseIp = HttpContext.Connection.RemoteIpAddress?.ToString()
            });

            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                accessToken = nouveauAccess,
                refreshToken = nouveauRefresh
            });
        }

        // POST /api/v1/auth/logout
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest req)
        {
            var token = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == req.RefreshToken);

            if (token != null)
            {
                token.Revoque = true;
                await _context.SaveChangesAsync();
            }

            return Ok(new { success = true, message = "Déconnecté." });
        }

        // GET /api/v1/auth/public-key-info
        [HttpGet("public-key-info")]
        public IActionResult PublicKeyInfo()
        {
            return Ok(new
            {
                issuer = _config["Jwt:Issuer"],
                audience = _config["Jwt:Audience"],
                algorithme = "HS256",
                dureeMinutes = int.Parse(_config["Jwt:DureeMinutes"] ?? "60"),
                refreshDureeJours = int.Parse(_config["Jwt:RefreshDureeJours"] ?? "7"),
                clockSkewSeconds = int.Parse(_config["Jwt:ClockSkew"] ?? "5"),
                notes = new[] 
                {
                    "Configuration JWT pour les microservices",
                    "Clé symétrique HMAC-SHA256. Partagez 'Jwt:Cle' via variable d'environnement sécurisée",
                    "Les autres services doivent utiliser ces mêmes paramètres pour valider les tokens",
                    "Les claims du token incluent: NameIdentifier, Email, Role, permissions"
                }
            });
        }

        // GET /api/v1/auth/current-user
        [HttpGet("current-user")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdStr, out var userId))
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Identifiant utilisateur invalide"
                    });
                }

                _logger.LogInformation("Récupération de l'utilisateur connecté {UserId}", userId);

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

                var currentUserInfo = new CurrentUserInfo
                {
                    Id = user.Id,
                    Nom = user.Nom,
                    Prenom = user.Prenom,
                    Email = user.Email,
                    Role = user.Role!.Nom,
                    Statut = user.Statut,
                    DateCreation = user.DateCreation
                };

                return Ok(new ApiResponse<CurrentUserInfo>
                {
                    Success = true,
                    Message = "Utilisateur connecté récupéré avec succès",
                    Data = currentUserInfo
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération de l'utilisateur connecté");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Erreur serveur lors de la récupération de l'utilisateur"
                });
            }
        }

        // GET /api/v1/auth/verify
        [HttpGet("verify")]
        [Authorize]
        public async Task<IActionResult> VerifyToken()
        {
            try
            {
                var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdStr, out var userId))
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Token invalide"
                    });
                }

                _logger.LogInformation("Vérification du token pour l'utilisateur {UserId}", userId);

                var user = await _context.Utilisateurs
                    .Where(u => u.Id == userId && u.Statut)
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Utilisateur non trouvé ou désactivé"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Token valide",
                    Data = new { userId = user.Id, email = user.Email }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la vérification du token");
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Erreur lors de la vérification du token"
                });
            }
        }
    }

    public record LoginRequest(string Email, string MotDePasse);
    public record RegisterRequest(string Nom, string Prenom, string Email, string MotDePasse, string ConfirmMotDePasse);
    public record RefreshTokenRequest(string RefreshToken);
    public record LogoutRequest(string RefreshToken);
}
