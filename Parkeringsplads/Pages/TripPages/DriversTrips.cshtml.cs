using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Parkeringsplads.Models;
using Parkeringsplads.Services.EFServices;
using Parkeringsplads.Services.Interfaces;

namespace Parkeringsplads.Pages.TripPages
{
    public class DriversTripsModel : PageModel
    {
        private readonly ParkeringspladsContext _context;
        private readonly ITripService _tripService;

        public DriversTripsModel(ParkeringspladsContext context, ITripService tripService)
        {
            _context = context;
            _tripService = tripService;
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

        public async Task<IActionResult> OnPostDeleteAsync(int tripId)
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

            var success = await _tripService.DeleteTripAsync(tripId, user.UserId);

            if (!success)
            {
                TempData["ErrorMessage"] = "Trip not found or you do not have permission to delete it.";
                return RedirectToPage();
            }

            TempData["SuccessMessage"] = "Trip deleted successfully.";
            return RedirectToPage();
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
