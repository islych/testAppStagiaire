using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Shared.Kernel
{
    /// <summary>
    /// Extension pour configurer JWT Bearer dans les microservices
    /// À utiliser dans tous les services pour une configuration cohérente
    /// </summary>
    public static class JwtExtensions
    {
        /// <summary>
        /// Ajoute l'authentification JWT Bearer avec les paramètres centralisés
        /// </summary>
        public static IServiceCollection AddJwtBearer(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var jwtSection = configuration.GetSection("Jwt");
            var issuer = jwtSection["Issuer"]
                ?? throw new InvalidOperationException("Jwt:Issuer not configured");
            var audience = jwtSection["Audience"]
                ?? throw new InvalidOperationException("Jwt:Audience not configured");
            var cle = jwtSection["Cle"]
                ?? throw new InvalidOperationException("Jwt:Cle not configured");
            var clockSkew = int.Parse(jwtSection["ClockSkew"] ?? "5");

            if (cle.Length < 32)
                throw new InvalidOperationException("Jwt:Cle must be at least 32 characters long");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(cle));

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = issuer,
                        ValidAudience = audience,
                        IssuerSigningKey = key,
                        ClockSkew = TimeSpan.FromSeconds(clockSkew)
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            var logger = context.HttpContext.RequestServices
                                .GetRequiredService<ILogger<object>>();
                            logger.LogError($"JWT Authentication failed: {context.Exception.Message}");
                            return Task.CompletedTask;
                        },
                        OnTokenValidated = context =>
                        {
                            var email = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                            var role = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
                            var logger = context.HttpContext.RequestServices
                                .GetRequiredService<ILogger<object>>();
                            logger.LogInformation($"JWT Token validated for user: {email} (Role: {role})");
                            return Task.CompletedTask;
                        }
                    };
                });

            return services;
        }

        /// <summary>
        /// Valide que la configuration JWT est complète
        /// Doit être appelé au démarrage de l'application
        /// </summary>
        public static void ValidateJwtConfiguration(IConfiguration configuration)
        {
            var jwtSection = configuration.GetSection("Jwt");
            var issuer = jwtSection["Issuer"];
            var audience = jwtSection["Audience"];
            var cle = jwtSection["Cle"];

            if (string.IsNullOrWhiteSpace(issuer))
                throw new InvalidOperationException("Jwt:Issuer is not configured");

            if (string.IsNullOrWhiteSpace(audience))
                throw new InvalidOperationException("Jwt:Audience is not configured");

            if (string.IsNullOrWhiteSpace(cle))
                throw new InvalidOperationException("Jwt:Cle is not configured");

            if (cle.Length < 32)
                throw new InvalidOperationException("Jwt:Cle must be at least 32 characters long for HMAC-SHA256");

            Console.WriteLine("✅ JWT Configuration is valid");
            Console.WriteLine($"   Issuer: {issuer}");
            Console.WriteLine($"   Audience: {audience}");
            Console.WriteLine($"   Algorithm: HS256");
        }
    }
}
