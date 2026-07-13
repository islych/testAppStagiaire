using Microsoft.AspNetCore.Mvc;

namespace Authentication.API.Controllers
{
    public class DirectionController : Controller
    {
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Role") != "Direction")
                return RedirectToAction("Login", "Account");

            ViewBag.Nom = HttpContext.Session.GetString("Nom");
            return View();
        }
    }
}
