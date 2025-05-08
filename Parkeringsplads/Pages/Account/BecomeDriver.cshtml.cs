using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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

        public IActionResult OnGet()
        {
            // Check if user is logged in
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToPage("./Login/Login");
            }

            // Check if user is already a driver
            var user = _context.User.FirstOrDefault(u => u.Email == userEmail);
            if (user != null && _context.Driver.Any(d => d.UserId == user.UserId))
            {
                TempData["ErrorMessage"] = "You are already registered as a driver.";
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

            var user = _context.User.FirstOrDefault(u => u.Email == userEmail);
            if (user == null)
            {
                return RedirectToPage("./Login/Login");
            }

            // Check if user is already a driver
            if (_context.Driver.Any(d => d.UserId == user.UserId))
            {
                TempData["ErrorMessage"] = "You are already registered as a driver.";
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

                // Set the session string "IsDriver" with the DriverId
                HttpContext.Session.SetString("IsDriver", driver.DriverId.ToString());

                TempData["SuccessMessage"] = "Successfully registered as a driver!";
                return RedirectToPage("/Account/Profile");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while registering as a driver. Please try again.");
                return Page();
            }
        }
    }
}
