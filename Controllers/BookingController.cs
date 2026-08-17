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

        public BookingController(KickFiveContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
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
            var
                currentUser = await _userManager.GetUserAsync(User);

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
        [ValidateAntiForgeryToken]
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

        [HttpGet]
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("Id, StartDateTime, EndDateTime, FieldId")] Booking booking)
        {
            var fields = await _context.Field.ToListAsync();
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return RedirectToPage("/Account/Login", new { Area = "Identity" });
            }

            if (ModelState.IsValid)
            {

                if (booking.EndDateTime <= booking.StartDateTime)
                {
                    ModelState.AddModelError(string.Empty, "End time must be after start time.");
                    ViewBag.Fields = fields;
                    return View(booking);
                }

                var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");


                if (!isAdmin && booking.StartDateTime < DateTime.Now.AddHours(24))
                {
                    ModelState.AddModelError(string.Empty, "You can't edit bookings within 24 hours of the start time.");
                    ViewBag.Fields = fields;
                    return View(booking);
                }
                try
                {
                    var existingBooking = await _context.Booking.FindAsync(booking.Id);
                    if (existingBooking == null)
                    {
                        return NotFound();
                    }

                    if (!isAdmin && existingBooking.UserId != currentUser.Id)
                    {
                        return Forbid();
                    }
                    existingBooking.StartDateTime = booking.StartDateTime;
                    existingBooking.EndDateTime = booking.EndDateTime;
                    existingBooking.FieldId = booking.FieldId;
                    existingBooking.Price = await CalculatePrice(booking.StartDateTime, booking.EndDateTime, booking.FieldId);

                    _context.Update(existingBooking);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingExists(booking.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"An error occurred while updating the booking: {ex.Message}");
                    ViewBag.Fields = fields;
                    return View(booking);
                }

                return RedirectToAction(nameof(Index));
            }

            ViewBag.Fields = fields;
            return View(booking);

        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var booking = await _context.Booking.FindAsync(id);

            if (booking == null)
            {
                return NotFound();
            }

            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");

            if (!isAdmin)
            {
                return Forbid();
            }

            return View(booking);
        }


        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if(currentUser == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var booking = await _context.Booking.FindAsync(id);
            if(booking == null)
            {
                return NotFound();
            }

            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");

            if(!isAdmin)
            {
                return Forbid();
            }


            _context.Booking.Remove(booking);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if(currentUser == null)             {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var booking = await _context.Booking.FindAsync(id);

            if (booking == null)
            {
                return NotFound();
            }

            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");

            if (!isAdmin && booking.UserId != currentUser.Id)
            {
                return Forbid();
            }

            if (!isAdmin && booking.StartDateTime < DateTime.Now.AddHours(24))
            {
                TempData["ErrorMessage"] = "You can't cancel bookings within 24 hours of the start time.";
                return RedirectToAction(nameof(Details), new { id = booking.Id });
            }

            booking.Status = "Cancelled";
            _context.Update(booking);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if(currentUser == null)
            {
                return RedirectToPage("/Account/Login", new {area = "Identity" });
            }

            var booking = await _context.Booking.FindAsync(id);

            if (booking == null)
            {
                return NotFound();
            }

            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");

            if(!isAdmin)
            {
                return Forbid();
            }

            booking.Status = "Confirmed";

            _context.Update(booking);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        private bool BookingExists(int id)
        {
            return _context.Booking.Any(e => e.Id == id);
        }
    }
}
