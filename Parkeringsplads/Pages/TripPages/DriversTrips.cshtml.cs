using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Parkeringsplads.Models;

namespace Parkeringsplads.Pages.TripPages
{
    public class DriversTripsModel : PageModel
    {
        private readonly ParkeringspladsContext _context;

        public DriversTripsModel(ParkeringspladsContext context)
        {
            _context = context;
        }

        public IEnumerable<Trip> Trips { get; set; }

        public IActionResult OnGet()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToPage("./Login");
            }

            var user = _context.User.FirstOrDefault(u => u.Email == userEmail);
            if (user == null)
            {
                return RedirectToPage("./Login");
            }

            var isDriver = _context.Driver.Any(d => d.UserId == user.UserId);
            if (!isDriver)
            {
                TempData["ErrorMessage"] = "You must be a driver to access this page.";
                return RedirectToPage("./Account/Profile");
            }

            // Load trips for the driver including related requests
            Trips = _context.Trip.Include(t => t.Requests).Include(t => t.Car).ThenInclude(c => c.Driver).Where(t => t.Car.Driver.UserId == user.UserId).ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToPage("./Login");
            }

            var user = _context.User.FirstOrDefault(u => u.Email == userEmail);
            if (user == null)
            {
                return RedirectToPage("./Login");
            }

            var isDriver = _context.Driver.Any(d => d.UserId == user.UserId);
            if (!isDriver)
            {
                TempData["ErrorMessage"] = "You must be a driver to access this page.";
                return RedirectToPage("./Account/Profile");
            }

            if (!ModelState.IsValid)
            {
                // Reload existing trips if validation fails
                Trips = _context.Trip.Include(t => t.Requests).Include(t => t.Car).ThenInclude(c => c.Driver).Where(t => t.Car.Driver.UserId == user.UserId).ToList();

                return Page();
            }

            return Page();
        }
    }
}
