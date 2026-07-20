using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Authentication.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Authentication.Infrastructure.Security.JwtTokenGenerator
{
    public class JwtTokenGenerator
    {
        private readonly IConfiguration _config;

        public JwtTokenGenerator(IConfiguration config)
        {
            _config = config;
        }

        public string GenererAccessToken(Utilisateur utilisateur, IEnumerable<string> permissions)
        {
            var cle = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Cle"]!));

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, utilisateur.Id.ToString()),
                new Claim(ClaimTypes.Email, utilisateur.Email),
                new Claim(ClaimTypes.Role, utilisateur.Role!.Nom),
                new Claim("prenom", utilisateur.Prenom),
                new Claim("nom", utilisateur.Nom)
            };

            // Ajouter le département si c'est un encadrant
            if (utilisateur.DepartementId.HasValue)
                claims.Add(new Claim("departementId", utilisateur.DepartementId.Value.ToString()));

            foreach (var permission in permissions)
                claims.Add(new Claim("permission", permission));

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    int.Parse(_config["Jwt:DureeMinutes"] ?? "60")),
                signingCredentials: new SigningCredentials(cle, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenererRefreshToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }
    }
}
