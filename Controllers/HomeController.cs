using System.Diagnostics;
using KickFive.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;


namespace KickFive.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IEmailSender _emailSender;
        public HomeController(ILogger<HomeController> logger, IEmailSender emailSender)
        {
            _logger = logger;
            _emailSender = emailSender;
        }

        public IActionResult Index()
        {
            return View();
        }


        public async Task<IActionResult> Contact(string email, string body)
        {
            if(string.IsNullOrEmpty(email) || string.IsNullOrEmpty(body) || !email.Contains("@"))
            {
                ViewBag.Message = "Please fill in all fields.";
                return View();
            }

            try
            {
                await _emailSender.SendEmailAsync(
                    "contact@kickfive.com",
                    "Contact Form Submission",
                    "Client mail: " + email + " " + body);

                ViewBag.Message = "Email sent successfully!";
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Failed to send email. Please try again later.";
            }
            return View();
        }

        public IActionResult Privacy()
        {
                        
            return View();
        }

        public IActionResult Terms()
        {
            return View();
        }




        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
