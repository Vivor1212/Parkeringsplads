using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Parkeringsplads.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Parkeringsplads.Pages.Admin
{
    public class AddRequestAdminModel : PageModel
    {
        private readonly ParkeringspladsContext _context;

        public AddRequestAdminModel(ParkeringspladsContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Request Request { get; set; } = new();

        [BindProperty]
        public int SelectedUserId { get; set; }

        [BindProperty]
        public int SelectedTripId { get; set; }

        public List<SelectListItem> Users { get; set; } = new();
        public List<Trip> TripList { get; set; } = new();

        public string? UserAddress { get; set; }




        public async Task<IActionResult> OnGetAsync()
        {
            await LoadDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {

            var isAdmin = HttpContext.Session.GetString("IsAdmin");

            if (string.IsNullOrEmpty(isAdmin) || isAdmin != "true")
            {
                return RedirectToPage("/Admin/NotAdmin");
            }

            await LoadDataAsync();

            if (SelectedUserId <= 0 || SelectedTripId <= 0)
            {
                TempData["ErrorMessage"] = "Vælg både bruger og tur.";
                return Page();
            }

            var user = await _context.User
                .Include(u => u.UserAddresses)
                .ThenInclude(ua => ua.Address)
                .FirstOrDefaultAsync(u => u.UserId == SelectedUserId);

            if (user?.UserAddresses.FirstOrDefault()?.Address is Address address)
            {
                Request.RequestAddress = $"{address.AddressRoad} {address.AddressNumber}, {address.City?.PostalCode} {address.City?.CityName}";
            }

            Request.UserId = SelectedUserId;
            Request.TripId = SelectedTripId;
            Request.RequestTime = TimeOnly.FromDateTime(DateTime.Now);
            Request.RequestStatus = null;

            _context.Request.Add(Request);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Anmodningen er oprettet!";
            return RedirectToPage("/Admin/AdminDashboard");
        }

        private async Task LoadDataAsync()
        {
            Users = await _context.User
                .Select(u => new SelectListItem
                {
                    Value = u.UserId.ToString(),
                    Text = $"{u.FirstName} {u.LastName} ({u.Email})"
                }).ToListAsync();

            TripList = await _context.Trip
    .Include(t => t.Car)
        .ThenInclude(c => c.Driver)
            .ThenInclude(d => d.User)
    .OrderByDescending(t => t.TripDate)
    .ThenBy(t => t.TripTime)
    .ToListAsync();

            if (SelectedUserId > 0)
            {
                var user = await _context.User
                    .Include(u => u.UserAddresses)
                        .ThenInclude(ua => ua.Address)
                            .ThenInclude(a => a.City)
                    .FirstOrDefaultAsync(u => u.UserId == SelectedUserId);

                var addr = user?.UserAddresses.FirstOrDefault()?.Address;
                if (addr != null)
                {
                    UserAddress = $"{addr.AddressRoad} {addr.AddressNumber}, {addr.City?.PostalCode} {addr.City?.CityName}";
                }
            }
        }
    }
}
