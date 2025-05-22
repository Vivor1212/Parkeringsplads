using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Parkeringsplads.Models;
using Parkeringsplads.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Parkeringsplads.Pages.TripPages
{
    public class AvailableTripsModel : PageModel
    {
        private readonly ITripService _tripService;
        private readonly ParkeringspladsContext _context;

        public AvailableTripsModel(ITripService tripService, ParkeringspladsContext context)
        {
            _tripService = tripService;
            _context = context;
        }

        public List<Trip> Trips { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? DirectionFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? DateFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? HourFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? CityFilter { get; set; }

        public List<SelectListItem> CityOptions { get; set; } = new();

        public string SchoolAddress { get; set; } = "";
        public string SchoolName { get; set; } = "";

        public List<SelectListItem> DirectionOptions { get; set; } = new()
        {
            new SelectListItem { Value = "", Text = "Alle" },
            new SelectListItem { Value = "ToSchool", Text = "Til skole" },
            new SelectListItem { Value = "FromSchool", Text = "Fra skole" }
        };

        public List<SelectListItem> HourOptions { get; set; } = Enumerable
            .Range(0, 24)
            .Select(h => new SelectListItem
            {
                Value = h.ToString(),
                Text = $"{h:00}:00"
            })
            .ToList();

        public async Task<IActionResult> OnGetAsync()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail)){
                return RedirectToPage("/Account/Login/Login");
            }

            var user = await _context.User
                .Include(u => u.School)
                    .ThenInclude(s => s.Address)
                .FirstOrDefaultAsync(u => u.Email == userEmail);

            SchoolAddress = user.School.Address.FullAddress;
            SchoolName = user.School.SchoolName;

            Trips = await _tripService.GetAllAvailableTripsAsync(
                DirectionFilter,
                DateFilter,
                HourFilter,
                CityFilter,
                SchoolAddress
            );

            var destinations = DirectionFilter switch
            {
                "ToSchool" => Trips.Select(t => t.FromDestination),
                "FromSchool" => Trips.Select(t => t.ToDestination),
                _ => Trips.SelectMany(t => new[] { t.FromDestination, t.ToDestination })
            };

            CityOptions = destinations
                .Where(d => !string.IsNullOrWhiteSpace(d))
                .Select(ExtractCity)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct()
                .OrderBy(c => c)
                .Select(c => new SelectListItem { Value = c, Text = c })
                .ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostRequestAsync(int tripId)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var user = await _context.User.FirstOrDefaultAsync(u => u.Email == userEmail);

            var alreadyRequested = await _context.Request
                .FirstOrDefaultAsync(r => r.TripId == tripId && r.UserId == user.UserId);

            if (alreadyRequested == null)
            {
                _context.Request.Add(new Request
                {
                    TripId = tripId,
                    UserId = user?.UserId,
                    RequestStatus = null,
                    RequestTime = TimeOnly.FromDateTime(DateTime.Now)
                });

                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }

        public string DisplayDestination(string? destination)
        {
            if (string.IsNullOrWhiteSpace(destination)) return "";

            var normalizedDest = destination.Trim().ToLower();
            var normalizedSchool = SchoolAddress.Trim().ToLower();

            return normalizedDest.Contains(normalizedSchool) ? SchoolName : destination;
        }

        private string ExtractCity(string? address)
        {
            if (string.IsNullOrWhiteSpace(address)) return "";
            var parts = address.Split(',');
            return parts.Length > 1 ? parts[^1].Trim() : "";
        }
    }
}
