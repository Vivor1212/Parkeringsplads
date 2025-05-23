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
    public class AvailableTripsModel : BasePageModel
    {
        private readonly ITripService _tripService;
        private readonly IRequestService _requestService;

        public AvailableTripsModel(IUser userService, ITripService tripService, IRequestService requestService) : base(userService)
        {
            _tripService = tripService;
            _requestService = requestService;
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
            var userResult = await _userService.ValidateUserAsync(HttpContext); // Assuming a generic ValidateUserAsync
            var redirectResult = await HandleValidationAndRedirect(userResult);
            if (redirectResult != null)
            {
                return redirectResult;
            }

            SchoolAddress = userResult.User.School.Address.FullAddress;
            SchoolName = userResult.User.School.SchoolName;

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
            var userResult = await _userService.ValidateUserAsync(HttpContext);
            var redirectResult = await HandleValidationAndRedirect(userResult);
            if (redirectResult != null)
            {
                return redirectResult;
            }

            var alreadyRequested = await _requestService.RequestExistsAsync(tripId, userResult.User.UserId);
            if (alreadyRequested)
            {
                TempData["ErrorMessage"] = "Du har allerede anmodet denne tur.";
                return RedirectToPage();
            }

            var request = new Request
            {
                TripId = tripId,
                UserId = userResult.User.UserId,
                RequestStatus = null,
                RequestTime = TimeOnly.FromDateTime(DateTime.Now)
            };

            await _requestService.CreateRequestAsync(request);
            TempData["SuccessMessage"] = "Anmodning blev sent.";
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
