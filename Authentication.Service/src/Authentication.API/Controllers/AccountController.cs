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
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(string nom, string prenom, string email, string motDePasse)
        {
            if (await _context.Utilisateurs.AnyAsync(u => u.Email == email))
            {
                ViewBag.Erreur = "Cet email est déjà utilisé.";
                return View();
            }

            var stagiaireRole = await _context.Roles.FirstAsync(r => r.Nom == "Stagiaire");

            _context.Utilisateurs.Add(new Utilisateur
            {
                Nom = nom,
                Prenom = prenom,
                Email = email,
                MotDePasseHash = BCrypt.Net.BCrypt.HashPassword(motDePasse),
                RoleId = stagiaireRole.Id,
                Statut = true
            });
            await _context.SaveChangesAsync();

            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult Login() => View();

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
                ViewBag.Erreur = "Compte désactivé. Contactez l'administrateur.";
                return View();
            }

            // Stocker la session
            HttpContext.Session.SetInt32("UtilisateurId", utilisateur.Id);
            HttpContext.Session.SetString("Nom", utilisateur.Prenom);
            HttpContext.Session.SetString("Role", utilisateur.Role!.Nom);

            // Redirection selon le rôle
            return utilisateur.Role.Nom switch
            {
                "Stagiaire"     => RedirectToAction("Index", "Stagiaire"),
                "Encadrant"     => RedirectToAction("Index", "Encadrant"),
                "Direction"     => RedirectToAction("Index", "Direction"),
                "Centre"        => RedirectToAction("Index", "Centre"),
                "RH"            => RedirectToAction("Index", "RH"),
                "Administrateur"=> RedirectToAction("Index", "Admin"),
                _               => RedirectToAction("Login")
            };
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
