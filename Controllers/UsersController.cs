using Microsoft.AspNetCore.Mvc;

namespace KickFive.Controllers
{
    public class UsersController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
