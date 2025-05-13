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

            Trip = _context.Trip.Include(t => t.Requests).ThenInclude(r => r.Users).FirstOrDefault(t => t.TripId == tripId && t.Driver.UserId == user.UserId);

            if (Trip == null)
            {
                TempData["ErrorMessage"] = "Trip not found or you do not have access it it.";
                return RedirectToPage("./DriversTrips");
            }

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

            try
            {
                await _requestService.AcceptRequestAsync(requestId);
                TempData["SuccessMessage"] = "Request accepted successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
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

            try
            {
                await _requestService.RejectRequestAsync(requestId);
                TempData["SuccessMessage"] = "Request rejected successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToPage(new { tripId });
        }
    }
}
