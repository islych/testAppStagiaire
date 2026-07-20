using Documents.Application.Interfaces;
using Documents.Application.Services;
using Documents.Domain.Interfaces;
using Documents.Infrastructure.ExternalServices;
using Documents.Infrastructure.Persistence;
using Documents.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ─────────────────────────────────────────────────────────────────────────────
// BASE DE DONNÉES
// ─────────────────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<DocumentsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ─────────────────────────────────────────────────────────────────────────────
// REPOSITORIES
// ─────────────────────────────────────────────────────────────────────────────
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();

// ─────────────────────────────────────────────────────────────────────────────
// APPLICATION SERVICES
// ─────────────────────────────────────────────────────────────────────────────
builder.Services.AddScoped<IDocumentService, DocumentService>();

// ─────────────────────────────────────────────────────────────────────────────
// EXTERNAL SERVICES
// ─────────────────────────────────────────────────────────────────────────────

// Client HTTP vers Notifications.Service
var notificationsApiUrl = builder.Configuration["ExternalApis:NotificationsService"]
    ?? "http://localhost:5132";

builder.Services.AddHttpClient<INotificationService, NotificationServiceClient>(client =>
{
    client.BaseAddress = new Uri(notificationsApiUrl);
    client.Timeout = TimeSpan.FromSeconds(10);
});

var authenticationApiUrl = builder.Configuration["ExternalApis:AuthenticationService"]
    ?? "https://localhost:7058";

builder.Services.AddHttpClient<IAuthenticationServiceClient, AuthenticationServiceClient>(client =>
{
    client.BaseAddress = new Uri(authenticationApiUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Client HTTP vers Candidatures.Service — vérifie si le stagiaire a une candidature acceptée
var candidaturesApiUrl = builder.Configuration["ExternalApis:CandidaturesService"]
    ?? "http://localhost:5296";

builder.Services.AddHttpClient<ICandidaturesServiceClient, CandidaturesServiceClient>(client =>
{
    client.BaseAddress = new Uri(candidaturesApiUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(15);
});

// ─────────────────────────────────────────────────────────────────────────────
// AUTHENTIFICATION JWT
// ─────────────────────────────────────────────────────────────────────────────
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
    });

builder.Services.AddAuthorization();

// ─────────────────────────────────────────────────────────────────────────────
// CONTROLLERS + JSON
// ─────────────────────────────────────────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Les enums sont sérialisés en string (ex: "EnAttente" au lieu de 0)
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// ─────────────────────────────────────────────────────────────────────────────
// SWAGGER
// ─────────────────────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ─────────────────────────────────────────────────────────────────────────────
// CORS
// ─────────────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ─────────────────────────────────────────────────────────────────────────────
// LIMITE DE TAILLE DES FICHIERS UPLOADÉS (10 Mo)
// ─────────────────────────────────────────────────────────────────────────────
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10 Mo
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 10 * 1024 * 1024; // 10 Mo
});

// ─────────────────────────────────────────────────────────────────────────────
// BUILD
// ─────────────────────────────────────────────────────────────────────────────
var app = builder.Build();

// ─────────────────────────────────────────────────────────────────────────────
// MIGRATION AUTO AU DÉMARRAGE
// ─────────────────────────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DocumentsDbContext>();
    db.Database.Migrate();
}

// ─────────────────────────────────────────────────────────────────────────────
// PIPELINE HTTP
// ─────────────────────────────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Documents.API v1");
        options.RoutePrefix = string.Empty; // Swagger à la racine
    });
}

app.UseCors("AllowAll");

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Servir les fichiers uploadés en statique (optionnel, pour un accès direct)
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
