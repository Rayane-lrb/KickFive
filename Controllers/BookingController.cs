using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace KickFive.Controllers
{
    [Authorize]
    public class BookingController : Controller
    {
        private readonly KickFiveContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<User> _roleManager;

        public BookingController(KickFiveContext context, UserManager<User> userManager, RoleManager<User> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");

            IEnumerable<Booking> bookings;

            if (isAdmin)
            {
                bookings = await _context.Booking.ToListAsync();
            }
            else
            {
                bookings = await _context.Booking.Where(b => b.UserId == currentUser.Id).ToListAsync();
            }
            return View(bookings);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");
            var booking = await _context.Booking.FirstOrDefaultAsync(b => b.Id == id);
            if (booking == null)
            {
                return NotFound();
            }

            if (!isAdmin && booking.UserId != currentUser.Id)
            {
                return Forbid();
            }
            return View(booking);
        }


        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }
            var fields = await _context.Field.ToListAsync();
            ViewBag.Fields = fields;
            return View();
        }

        private async Task<decimal> CalculatePrice(DateTime startDateTime, DateTime endDateTime, int fieldId)
        {
            var field = await _context.Field.FindAsync(fieldId);
            if (field == null)
            {
                throw new Exception("Field not found");
            }
            var duration = endDateTime - startDateTime;
            var hours = (decimal)duration.TotalHours;
            return hours * 80;
        }

        [HttpPost]
        public async Task<IActionResult> Create([Bind("StartDateTime, EndDateTime, FieldId")] Booking booking)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            if (!ModelState.IsValid || booking.EndDateTime <= booking.StartDateTime)
            {
                if (booking.EndDateTime <= booking.StartDateTime)
                {
                    ModelState.AddModelError(string.Empty, "End time must be after start time.");
                }

                var fields = await _context.Field.ToListAsync();
                ViewBag.Fields = fields;
                return View(booking);
            }

            try
            {

                var newBooking = new Booking
                {
                    StartDateTime = booking.StartDateTime,
                    EndDateTime = booking.EndDateTime,
                    FieldId = booking.FieldId,
                    UserId = currentUser.Id,
                    Status = "Pending",
                    Price = await CalculatePrice(booking.StartDateTime, booking.EndDateTime, booking.FieldId)
                };

                _context.Add(newBooking);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred while creating the booking: {ex.Message}");
                var fields = await _context.Field.ToListAsync();
                ViewBag.Fields = fields;
                return View(booking);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var booking = await _context.Booking.FindAsync(id);
            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");

            if (booking == null)
            {
                return NotFound();
            }

            if (isAdmin == false)
            {
                if(booking.UserId!= currentUser.Id)
                {
                    return Forbid();
                }

                return View(booking);

            }

            return View(booking);

        }
    }
}