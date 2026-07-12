using Microsoft.AspNetCore.Mvc;

namespace Authentication.API.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}