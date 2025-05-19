using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Parkeringsplads.Models;
using Parkeringsplads.Services.Interfaces;

namespace Parkeringsplads.Pages.TripPages
{
    public class TripDetailsModel : PageModel
    {
        private readonly ParkeringspladsContext _context;
        private readonly IRequestService _requestService;

        public TripDetailsModel(ParkeringspladsContext context, IRequestService requestService)
        {
            _context = context;
            _requestService = requestService;
        }

        public Trip Trip { get; set; }
        public IEnumerable<Car> DriverCars { get; set; }

        public IActionResult OnGet(int tripId)
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

            var isDriver = _context.Driver.Any(d => d.UserId == user.UserId);
            if (!isDriver)
            {
                TempData["ErrorMessage"] = "You must be a driver to access this page.";
                return RedirectToPage("./Account/Profile");
            }

            Trip = _context.Trip.Include(t => t.Requests).ThenInclude(r => r.Users).Include(t => t.Car).ThenInclude(c => c.Driver).FirstOrDefault(t => t.TripId == tripId && t.Car != null && t.Car.Driver != null && t.Car.Driver.UserId == user.UserId);

            if (Trip == null)
            {
                TempData["ErrorMessage"] = "Trip not found or you do not have access it it.";
                return RedirectToPage("./DriversTrips");
            }

            DriverCars = Trip.Car != null ? _context.Car.Where(c => c.DriverId == Trip.Car.DriverId).ToList() : new List<Car>();

            return Page();
        }

        public async Task<IActionResult> OnPostAcceptRequestAsync(int tripId, int requestId)
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

            var isDriver = await _context.Driver.AnyAsync(d => d.UserId == user.UserId);
            if (!isDriver)
            {
                TempData["ErrorMessage"] = "You must be a driver to perfom this action.";
                return RedirectToPage("./Account/Profile");
            }

            var trip = await _context.Trip.Include(t => t.Requests).Include(t => t.Car).ThenInclude(c => c.Driver).FirstOrDefaultAsync(t => t.TripId == tripId && t.Car != null && t.Car.Driver != null && t.Car.Driver.UserId == user.UserId);
            if (trip == null)
            {
                TempData["ErrorMessage"] = "Trip not found or you do not have access to it.";
                return RedirectToPage("./TripPages/DriversTrips");
            }

            var acceptedRequests = trip.Requests.Count(r => r.RequestStatus == true);
            if (acceptedRequests >= trip.TripSeats)
            {
                TempData["ErrorMessage"] = "Cannot accept request: Trip is full.";
                return RedirectToPage(new { tripId });
            }

            try
            {
                await _requestService.AcceptRequestAsync(requestId);
                TempData["SuccessMessage"] = "Request accepted successfully.";
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred while accepting the request. Please try again.";
            }

            return RedirectToPage(new { tripId });
        }

        public async Task<IActionResult> OnPostRejectRequestAsync(int tripId, int requestId)
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

            var isDriver = await _context.Driver.AnyAsync(d => d.UserId == user.UserId);
            if (!isDriver)
            {
                TempData["ErrorMessage"] = "You must be a driver to perfom this action.";
                return RedirectToPage("./Account/Profile");
            }

            var trip = await _context.Trip.Include(t => t.Requests).Include(t => t.Car).ThenInclude(c => c.Driver).FirstOrDefaultAsync(t => t.TripId == tripId && t.Car != null && t.Car.Driver != null && t.Car.Driver.UserId == user.UserId);
            if (trip == null)
            {
                TempData["ErrorMessage"] = "Trip not found or you do not have access to it.";
                return RedirectToPage("./TripPages/DriversTrips");
            }

            try
            {
                await _requestService.RejectRequestAsync(requestId);
                TempData["SuccessMessage"] = "Request rejected successfully.";
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred while rejecting the request. Please try again.";
            }

            return RedirectToPage(new { tripId });
        }
    }
}
