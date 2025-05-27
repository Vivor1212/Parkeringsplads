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
            var userResult = await _userService.ValidateUserAsync(HttpContext);
            var redirectResult = await HandleValidationAndRedirect(userResult);
            if (redirectResult != null)
            {
                return redirectResult;
            }

            SchoolName = userResult.User.School.SchoolName;
            SchoolAddress = userResult.User.School.Address.FullAddress;


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
                .Select(_tripService.ExtractCity)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(c => c)
                .Select(c => new SelectListItem { Value = c, Text = c })
                .ToList();

            Console.WriteLine("SchoolAddress: " + userResult.User.School.Address.FullAddress);
            Console.WriteLine("City null?: " + (userResult.User.School.Address.City == null));

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
            return destination == SchoolAddress ? SchoolName : destination ?? "";
        }




    }
}
