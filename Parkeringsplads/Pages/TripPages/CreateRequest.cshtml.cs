using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Parkeringsplads.Models;
using Parkeringsplads.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace Parkeringsplads.Pages.TripPages
{
    public class CreateRequestModel : PageModel
    {
        private readonly ParkeringspladsContext _context;
        private readonly IRequestService _requestService;

        public CreateRequestModel(ParkeringspladsContext context, IRequestService requestService)
        {
            _context = context;
            _requestService = requestService;
        }

        [BindProperty]
        public Trip Trip { get; set; }

        [BindProperty]
        public string? Message { get; set; }

        [BindProperty]
        public string? Address { get; set; }
        public async Task<IActionResult> OnGetAsync(int tripId)
        {
            Trip = await _context.Trip
                .Include(t => t.Car)
                    .ThenInclude(c => c.Driver)
                        .ThenInclude(d => d.User)
                .FirstOrDefaultAsync(t => t.TripId == tripId);

            if (Trip == null)
                return NotFound();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int tripId)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
                return RedirectToPage("/Account/Login");

            var user = await _context.User.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (user == null)
                return RedirectToPage("/Account/Login");

            var alreadyRequested = await _context.Request
                .AnyAsync(r => r.TripId == tripId && r.UserId == user.UserId);

            var request = new Request
            {
                TripId = tripId,
                UserId = user.UserId,
                RequestTime = TimeOnly.FromDateTime(DateTime.Now),
                RequestMessage = Message,
                RequestAddress = Address
            };

            await _requestService.CreateRequestAsync(request);
            return RedirectToPage("/TripPages/AvailableTrips");

        }

    }
}
