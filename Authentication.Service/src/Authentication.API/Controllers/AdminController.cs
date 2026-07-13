using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Authentication.Infrastructure.Persistence;
using Authentication.Domain.Entities;

namespace Authentication.API.Controllers
{
    public class AdminController : Controller
    {
        private readonly AuthenticationDbContext _context;

        public AdminController(AuthenticationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("Role") != "Administrateur")
                return RedirectToAction("Login", "Account");

            ViewBag.Nom = HttpContext.Session.GetString("Nom");
            var utilisateurs = await _context.Utilisateurs.Include(u => u.Role).ToListAsync();
            return View(utilisateurs);
        }

        [HttpGet]
        public async Task<IActionResult> CreerCompte()
        {
            if (HttpContext.Session.GetString("Role") != "Administrateur")
                return RedirectToAction("Login", "Account");

            // Rôles disponibles pour l'admin (pas Stagiaire, il s'inscrit lui-même)
            ViewBag.Roles = await _context.Roles
                .Where(r => r.Nom != "Stagiaire")
                .ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreerCompte(string nom, string prenom, string email, int roleId)
        {
            if (HttpContext.Session.GetString("Role") != "Administrateur")
                return RedirectToAction("Login", "Account");

            if (await _context.Utilisateurs.AnyAsync(u => u.Email == email))
            {
                ViewBag.Erreur = "Cet email est déjà utilisé.";
                ViewBag.Roles = await _context.Roles.Where(r => r.Nom != "Stagiaire").ToListAsync();
                return View();
            }

            // Mot de passe temporaire
            var motDePasseTemp = "Temp@1234";

            _context.Utilisateurs.Add(new Utilisateur
            {
                Nom = nom,
                Prenom = prenom,
                Email = email,
                MotDePasseHash = BCrypt.Net.BCrypt.HashPassword(motDePasseTemp),
                RoleId = roleId,
                Statut = true
            });
            await _context.SaveChangesAsync();

            ViewBag.Success = $"Compte créé. Mot de passe temporaire : {motDePasseTemp}";
            ViewBag.Roles = await _context.Roles.Where(r => r.Nom != "Stagiaire").ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatut(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Administrateur")
                return RedirectToAction("Login", "Account");

            var utilisateur = await _context.Utilisateurs.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == id);
            if (utilisateur == null) return NotFound();

            // Empêcher la désactivation du dernier admin actif
            if (utilisateur.Role!.Nom == "Administrateur" && utilisateur.Statut)
            {
                var nbAdminsActifs = await _context.Utilisateurs
                    .Include(u => u.Role)
                    .CountAsync(u => u.Role!.Nom == "Administrateur" && u.Statut);

                if (nbAdminsActifs <= 1)
                {
                    TempData["Erreur"] = "Impossible de désactiver le dernier administrateur actif.";
                    return RedirectToAction("Index");
                }
            }

            utilisateur.Statut = !utilisateur.Statut;
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
