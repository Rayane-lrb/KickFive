using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace KickFive.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<User> _userManager;


        public UsersController(RoleManager<IdentityRole> roleManager, UserManager<User> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;

        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }


        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.roles = roles;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FirstName, LastName, Email, PhoneNumber")] User user, string Password, string? role)
        {
            if (!ModelState.IsValid)
            {
                var roles = await _roleManager.Roles.ToListAsync();
                ViewBag.Roles = roles;
                return View(user);
            }

            user.UserName = user.Email;
            var result = await _userManager.CreateAsync(user, Password);

            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(role) && await _roleManager.RoleExistsAsync(role))
                {
                    await _userManager.AddToRoleAsync(user, role);
                }
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            var rolesList = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = rolesList;
            return View(user);

        }


        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {

            if(id == null)
            {
                return NotFound();
            }
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.roles = roles;

            var currentRole = await _userManager.GetRolesAsync(user);
            ViewBag.currentRole = currentRole.FirstOrDefault();

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("Id, FirstName, LastName, Email, PhoneNumber")] User user, string? role)
        {
            if (!ModelState.IsValid)
            {
                var roles = await _roleManager.Roles.ToListAsync();
                ViewBag.Roles = roles;
                return View(user);
            }

            var existingUser = await _userManager.FindByIdAsync(user.Id);
            

            if (existingUser != null)
            {

                try
                {
                    existingUser.FirstName = user.FirstName;
                    existingUser.LastName = user.LastName;
                    existingUser.Email = user.Email;
                    existingUser.PhoneNumber = user.PhoneNumber;
                    var result = await _userManager.UpdateAsync(existingUser);

                    if (!result.Succeeded)
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        var roles = await _roleManager.Roles.ToListAsync();
                        ViewBag.Roles = roles;
                        return View(user);
                    }

                    if (!string.IsNullOrEmpty(role) && await _roleManager.RoleExistsAsync(role))
                    {
                        var currentRoles = await _userManager.GetRolesAsync(existingUser);
                        await _userManager.RemoveFromRolesAsync(existingUser, currentRoles);
                        await _userManager.AddToRoleAsync(existingUser, role);
                    }
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    ModelState.AddModelError(string.Empty, "This user was modified by someone else. Please reload and try again.");
                    return View(user);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "An error occurred while updating the user. Please try again.");
                    return View(user);
                }
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction(nameof(Index));
            }
            else
            {
                ModelState.AddModelError(string.Empty, "An error occurred while deleting the user. Please try again.");
                return View(user);

            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Block(string id)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");

            if(!isAdmin)
            {
                return Forbid();
            }

            if (currentUser.Id == id)
            {
                TempData["ErrorMessage"] = "You cannot block your own account.";
                return RedirectToAction(nameof(Index));
            }

            var userToBlock = await _userManager.FindByIdAsync(id);

            if (userToBlock == null)
            {
                return NotFound();
            }

            if (await _userManager.IsLockedOutAsync(userToBlock))
            {
                TempData["ErrorMessage"] = "This user is already blocked.";
                return RedirectToAction(nameof(Index));
            }

            if (!await _userManager.GetLockoutEnabledAsync(userToBlock))
            {
                await _userManager.SetLockoutEnabledAsync(userToBlock, true);
            }

            var result = await _userManager.SetLockoutEndDateAsync(userToBlock, DateTimeOffset.MaxValue);

            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = "An error occurred while blocking the user. Please try again.";
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnBlock(string id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if(currentUser == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");

            if(!isAdmin)
            {
                return Forbid();
            }

            var userToUnblock = await _userManager.FindByIdAsync(id);

            if(userToUnblock == null)
            {
                return NotFound();
            }

            if(await _userManager.IsLockedOutAsync(userToUnblock) == false)
            {
                TempData["ErrorMessage"] = "This user is not currently blocked.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _userManager.SetLockoutEndDateAsync(userToUnblock, null);
            
            if(!result.Succeeded)
            {
                TempData["ErrorMessage"] = "An error occurred while unblocking the user. Please try again.";
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Index));
        }
    }
}