using Microsoft.AspNetCore.Mvc;

namespace Authentication.API.Controllers
{
    public class StagiaireController : Controller
    {
        public IActionResult Index(string nom)
        {
            ViewBag.Nom = nom;
            return View();
        }
    }
}