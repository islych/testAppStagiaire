using Microsoft.AspNetCore.Mvc;

namespace Authentication.API.Controllers
{
    public class EncadrantController : Controller
    {
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Role") != "Encadrant")
                return RedirectToAction("Login", "Account");

            ViewBag.Nom = HttpContext.Session.GetString("Nom");
            return View();
        }
    }
}
