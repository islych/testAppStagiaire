using Microsoft.AspNetCore.Mvc;

namespace Authentication.API.Controllers
{
    public class RHController : Controller
    {
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Role") != "RH")
                return RedirectToAction("Login", "Account");

            ViewBag.Nom = HttpContext.Session.GetString("Nom");
            return View();
        }
    }
}
