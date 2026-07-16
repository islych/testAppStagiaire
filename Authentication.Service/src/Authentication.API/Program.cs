using Authentication.Infrastructure.Persistence;
using Authentication.Infrastructure.Security.JwtTokenGenerator;
using Authentication.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AuthenticationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<JwtTokenGenerator>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtCle = builder.Configuration["Jwt:Cle"]!;
        var clockSkew = int.Parse(builder.Configuration["Jwt:ClockSkew"] ?? "5");
        
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtCle)),
            ClockSkew = TimeSpan.FromSeconds(clockSkew)
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine($"Token validated for: {context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value}");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS pour permettre aux autres services de consommer l'API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

var app = builder.Build();

// Seed au démarrage
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AuthenticationDbContext>();
    db.Database.Migrate();

    // Seed rôles
    var rolesRequis = new[] { "Stagiaire", "Encadrant", "Direction", "Centre", "RH", "Administrateur" };
    foreach (var nom in rolesRequis)
        if (!db.Roles.Any(r => r.Nom == nom))
            db.Roles.Add(new Role { Nom = nom });
    db.SaveChanges();

    // Seed permissions
    var permissionsRequises = new[]
    {
        ("UTILISATEUR_CONSULTER",   "Consulter le détail d'un utilisateur"),
        ("UTILISATEUR_LISTER",      "Lister l'ensemble des utilisateurs"),
        ("UTILISATEUR_CREER",       "Créer un compte"),
        ("UTILISATEUR_MODIFIER",    "Modifier un utilisateur"),
        ("UTILISATEUR_SUPPRIMER",   "Supprimer un utilisateur"),
        ("UTILISATEUR_ACTIVER_DESACTIVER", "Activer/désactiver un compte"),
        ("UTILISATEUR_CHANGER_ROLE","Modifier le rôle d'un utilisateur"),
        ("ROLE_CONSULTER",          "Consulter les rôles"),
        ("ROLE_GERER_PERMISSIONS",  "Gérer les permissions d'un rôle"),
        ("PERMISSION_CONSULTER",    "Consulter le référentiel des permissions")
    };

    foreach (var (code, desc) in permissionsRequises)
        if (!db.Permissions.Any(p => p.Code == code))
            db.Permissions.Add(new Permission { Code = code, Description = desc });
    db.SaveChanges();

    // Associer toutes les permissions à l'Administrateur
    var adminRole = db.Roles.First(r => r.Nom == "Administrateur");
    var toutesPermissions = db.Permissions.ToList();
    foreach (var perm in toutesPermissions)
    {
        if (!db.RolePermissions.Any(rp => rp.RoleId == adminRole.Id && rp.PermissionId == perm.Id))
            db.RolePermissions.Add(new RolePermission { RoleId = adminRole.Id, PermissionId = perm.Id });
    }
    db.SaveChanges();

    // Compte admin par défaut
    if (!db.Utilisateurs.Any(u => u.RoleId == adminRole.Id))
    {
        db.Utilisateurs.Add(new Utilisateur
        {
            Nom = "Admin",
            Prenom = "Super",
            Email = "admin@system.com",
            MotDePasseHash = BCrypt.Net.BCrypt.HashPassword("Admin@1234"),
            RoleId = adminRole.Id,
            Statut = true
        });
        db.SaveChanges();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseRouting();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
