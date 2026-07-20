using Candidatures.Application.Interfaces;
using Candidatures.Application.Services;
using Candidatures.Domain.Interfaces;
using Candidatures.Infrastructure.ExternalServices;
using Candidatures.Infrastructure.Persistence;
using Candidatures.Infrastructure.Repositories;using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// DbContext
builder.Services.AddDbContext<CandidaturesDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories
builder.Services.AddScoped<ICandidatureRepository, CandidatureRepository>();

// Application Services
builder.Services.AddScoped<ICandidatureService, CandidatureService>();
builder.Services.AddScoped<IMasterDataService, MasterDataService>();
builder.Services.AddScoped<IUserLookupService, UserLookupService>();

// External Services — client HTTP vers Notifications.Service
var notificationsApiUrl = builder.Configuration["ExternalApis:NotificationsService"]
    ?? "http://localhost:5132";

builder.Services.AddHttpClient<INotificationService, NotificationServiceClient>(client =>
{
    client.BaseAddress = new Uri(notificationsApiUrl);
    client.Timeout = TimeSpan.FromSeconds(10);
});

// ⭐ HttpClient pour Authentication.Service
var authenticationApiUrl = builder.Configuration["ExternalApis:AuthenticationService"] 
    ?? "https://localhost:7058";

builder.Services.AddHttpClient<IAuthenticationServiceClient, AuthenticationServiceClient>(
    client =>
    {
        client.BaseAddress = new Uri(authenticationApiUrl);
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        client.Timeout = TimeSpan.FromSeconds(30);
    });

// JWT Authentication
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
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS pour permettre à l'app web et autres services de consommer cette API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Database Migration - auto-création de la base au démarrage
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CandidaturesDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

