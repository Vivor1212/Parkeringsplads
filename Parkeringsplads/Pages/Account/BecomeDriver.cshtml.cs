using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Parkeringsplads.Models;
using Parkeringsplads.Services.Interfaces;

namespace Parkeringsplads.Pages.Account
{
    public class BecomeDriverModel : PageModel
    {
        private readonly ParkeringspladsContext _context;
        private readonly IDriverService _driverService;

        public BecomeDriverModel(ParkeringspladsContext context, IDriverService driverService)
        {
            _context = context;
            _driverService = driverService;
        }

        [BindProperty]
        public Driver Driver { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToPage("./Login/Login");
            }

            var user = await _context.User.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (user != null && await _context.Driver.AnyAsync(d => d.UserId == user.UserId))
            {
                TempData["ErrorMessage"] = "Du er allerede registeret som kører.";
                return RedirectToPage("/Account/Profile");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToPage("./Login/Login");
            }

            var user = await _context.User.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (user == null)
            {
                return RedirectToPage("./Login/Login");
            }

            if ( await _context.Driver.AnyAsync(d => d.UserId == user.UserId))
            {
                TempData["ErrorMessage"] = "Du er allerede registeret som kører.";
                return RedirectToPage("/Account/Profile");
            }

            var driver = new Driver
            {
                UserId = user.UserId,
                DriverLicense = Driver.DriverLicense,
                DriverCpr = Driver.DriverCpr
            };

            try
            {
                await _driverService.CreateDriverAsync(driver);

                HttpContext.Session.SetString("IsDriver", driver.DriverId.ToString());

                TempData["SuccessMessage"] = "Du er nu registeret som en kører. Husk at registere din bil <a href='/CarPages/Car'>her</a>.";
                return RedirectToPage("/Account/Profile");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Der skete en fejl mens du prøvede at registere som en kører. Prøve venligst igen.");
                return Page();
            }
        }
    }
}
