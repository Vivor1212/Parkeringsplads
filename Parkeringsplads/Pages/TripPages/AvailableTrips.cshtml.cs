using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Parkeringsplads.Models;

namespace Parkeringsplads.Pages.TripPages
{
    public class AvailableTripsModel : PageModel
    {
        private readonly ParkeringspladsContext _context;

        public AvailableTripsModel(ParkeringspladsContext context)
        {
            _context = context;
        }

        public List<Parkeringsplads.Models.Trip> Trips { get; set; }

        public async Task OnGetAsync()
        {
            Trips = await _context.Trip
                .Include(t => t.Driver)
                    .ThenInclude(d => d.User)
                .OrderBy(t => t.TripDate)
                .ThenBy(t => t.TripTime)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostRequestAsync(int tripId)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail)) return RedirectToPage("/Account/Login/Login");

            var user = await _context.User.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (user == null) return Unauthorized();

            var alreadyRequested = await _context.Request
                .FirstOrDefaultAsync(r => r.TripId == tripId && r.UserId == user.UserId);

            if (alreadyRequested == null)
            {
                _context.Request.Add(new Request
                {
                    TripId = tripId,
                    UserId = user.UserId,
                    RequestStatus = null // Pending
                });
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }
    }
}
