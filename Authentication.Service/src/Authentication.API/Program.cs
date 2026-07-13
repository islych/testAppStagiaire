using Authentication.Infrastructure.Persistence;
using Authentication.Domain.Entities;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AuthenticationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllersWithViews();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Seed des 6 rôles au démarrage
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AuthenticationDbContext>();
    db.Database.Migrate();

    var rolesRequis = new[] { "Stagiaire", "Encadrant", "Direction", "Centre", "RH", "Administrateur" };
    foreach (var nom in rolesRequis)
    {
        if (!db.Roles.Any(r => r.Nom == nom))
            db.Roles.Add(new Role { Nom = nom });
    }
    db.SaveChanges();

    // Compte admin par défaut si aucun admin n'existe
    var adminRole = db.Roles.First(r => r.Nom == "Administrateur");
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

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
