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
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToPage("./Login/Login");
            }

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

            if (_context.Driver.Any(d => d.UserId == user.UserId))
            {
                TempData["ErrorMessage"] = "Du er allerede chauffør.";
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

                TempData["SuccessMessage"] = "Du er nu oprettet som chauffør.";
                return RedirectToPage("/Account/Profile");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Der skete en fejl" + ex.Message;
                return Page();
            }
        }
    }
}
