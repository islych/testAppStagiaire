using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using GestionDesStagiaires.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Force Development mode par défaut (évite les problèmes de configuration)
if (string.IsNullOrEmpty(builder.Environment.EnvironmentName) || 
    builder.Environment.EnvironmentName == "Production")
{
    builder.Environment.EnvironmentName = "Development";
}

// Configuration des URLs des APIs
var apiUrls = builder.Configuration.GetSection("ApiUrls");
var authenticationApiUrl = apiUrls["AuthenticationApi"] ?? "http://localhost:5174";
var candidaturesApiUrl = apiUrls["CandidaturesApi"] ?? "http://localhost:5296";

// Add services to the container
builder.Services.AddRazorPages();
builder.Services.AddControllers();

// Configuration de la protection des données
builder.Services.AddDataProtection()
    .SetDefaultKeyLifetime(TimeSpan.FromDays(90))
    .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "keys")));

// Configuration de l'authentification par cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/account/login";
        options.LogoutPath = "/account/logout";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.Name = "GestionDesStagiaires.Auth";
    });

builder.Services.AddAuthorization();

// Configuration des HttpClientFactory avec typed clients
// IMPORTANT: AddHttpClient enregistre automatiquement l'interface, pas besoin de AddScoped ensuite
builder.Services.AddHttpClient<IAuthenticationApiService, AuthenticationApiService>(client =>
{
    client.BaseAddress = new Uri(authenticationApiUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient<ICandidaturesApiService, CandidaturesApiService>(client =>
{
    client.BaseAddress = new Uri(candidaturesApiUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Service de gestion des utilisateurs
builder.Services.AddHttpClient<IUserManagementService, UserManagementService>(client =>
{
    client.BaseAddress = new Uri(authenticationApiUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Service d'enregistrement
builder.Services.AddHttpClient<IRegistrationService, RegistrationService>(client =>
{
    client.BaseAddress = new Uri(authenticationApiUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Service des données maîtres (Master Data) depuis l'API
builder.Services.AddHttpClient<IMasterDataApiService, MasterDataApiService>(client =>
{
    client.BaseAddress = new Uri(candidaturesApiUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// CORS pour les appels API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMicroservices", policy =>
    {
        policy.WithOrigins(authenticationApiUrl, candidaturesApiUrl)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Ajout de session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.Name = "GestionDesStagiaires.Session";
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    // En développement, autoriser les requêtes non-HTTPS
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Authentification & Autorisation
app.UseAuthentication();
app.UseAuthorization();

// Session
app.UseSession();

app.MapRazorPages();
app.MapControllers();

app.Run();
