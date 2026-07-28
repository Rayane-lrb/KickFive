using Microsoft.AspNetCore.Mvc;

namespace KickFive.Controllers
{
    public class BookingController1 : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
