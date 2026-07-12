using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Authentication.Infrastructure.Persistence;
using Authentication.Domain.Entities;

namespace Authentication.API.Controllers
{
    public class AccountController : Controller
    {
        private readonly AuthenticationDbContext _context;

        public AccountController(AuthenticationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string nom, string prenom, string email, string motDePasse)
        {
            if (await _context.Utilisateurs.AnyAsync(u => u.Email == email))
            {
                ViewBag.Erreur = "Cet email est déjà utilisé.";
                return View();
            }

            var stagiaireRole = await _context.Roles.FirstOrDefaultAsync(r => r.Nom == "Stagiaire");
            if (stagiaireRole == null)
            {
                stagiaireRole = new Role { Nom = "Stagiaire" };
                _context.Roles.Add(stagiaireRole);
                await _context.SaveChangesAsync();
            }

            var utilisateur = new Utilisateur
            {
                Nom = nom,
                Prenom = prenom,
                Email = email,
                MotDePasseHash = BCrypt.Net.BCrypt.HashPassword(motDePasse),
                RoleId = stagiaireRole.Id,
                Statut = true
            };

            _context.Utilisateurs.Add(utilisateur);
            await _context.SaveChangesAsync();

            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string motDePasse)
        {
            var utilisateur = await _context.Utilisateurs
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email);

            if (utilisateur == null || !BCrypt.Net.BCrypt.Verify(motDePasse, utilisateur.MotDePasseHash))
            {
                ViewBag.Erreur = "Email ou mot de passe incorrect.";
                return View();
            }

            if (!utilisateur.Statut)
            {
                ViewBag.Erreur = "Compte désactivé.";
                return View();
            }

            return RedirectToAction("Index", "Stagiaire", new { nom = utilisateur.Prenom });
        }
    }
}