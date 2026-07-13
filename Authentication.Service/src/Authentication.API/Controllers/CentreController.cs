using Microsoft.AspNetCore.Mvc;

namespace Authentication.API.Controllers
{
    public class CentreController : Controller
    {
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Role") != "Centre")
                return RedirectToAction("Login", "Account");

            ViewBag.Nom = HttpContext.Session.GetString("Nom");
            return View();
        }
    }
}
