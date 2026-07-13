using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Authentication.Infrastructure.Persistence;
using Authentication.Domain.Entities;
using Authentication.Infrastructure.Security.JwtTokenGenerator;

namespace Authentication.API.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthApiController : ControllerBase
    {
        private readonly AuthenticationDbContext _context;
        private readonly JwtTokenGenerator _jwt;
        private readonly IConfiguration _config;

        public AuthApiController(AuthenticationDbContext context, JwtTokenGenerator jwt, IConfiguration config)
        {
            _context = context;
            _jwt = jwt;
            _config = config;
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
                return Unauthorized(new { success = false, message = "Identifiants invalides." });

            if (!utilisateur.Statut)
                return StatusCode(403, new { success = false, message = "Compte désactivé." });

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

            return Ok(new
            {
                success = true,
                accessToken,
                refreshToken = refreshTokenValeur,
                expiration = DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:DureeMinutes"] ?? "60")),
                utilisateur = new
                {
                    utilisateur.Id,
                    utilisateur.Nom,
                    utilisateur.Prenom,
                    utilisateur.Email,
                    role = utilisateur.Role.Nom,
                    permissions
                }
            });
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
                note = "Clé symétrique HMAC-SHA256. Partagez Jwt:Cle avec les autres microservices via variable d'environnement sécurisée."
            });
        }
    }

    public record LoginRequest(string Email, string MotDePasse);
    public record RefreshTokenRequest(string RefreshToken);
    public record LogoutRequest(string RefreshToken);
}
