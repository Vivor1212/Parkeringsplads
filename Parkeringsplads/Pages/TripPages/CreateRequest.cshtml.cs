using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Parkeringsplads.Models;
using Parkeringsplads.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace Parkeringsplads.Pages.TripPages
{
    public class CreateRequestModel : BasePageModel
    {
        private readonly ITripService _tripService;
        private readonly IRequestService _requestService;

        public CreateRequestModel(IUser userService,ITripService tripService, IRequestService requestService) : base(userService)
        {
            _tripService = tripService;
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
            Trip = await _tripService.GetTripByIdAsync(tripId);
            if (Trip == null)
                return NotFound();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int tripId)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
                return RedirectToPage("/Account/Login");

            var user = await _userService.GetUserWithDetailsByEmailAsync(userEmail);
            if (user == null)
                return RedirectToPage("/Account/Login");

            var alreadyRequested = await _requestService.RequestExistsAsync(tripId, user.UserId);
            if (alreadyRequested)
            {
                TempData["ErrorMessage"] = "Du har allerede anmodet om denne tur.";
                return RedirectToPage("/TripPages/AvailableTrips");
            }

            var request = new Request
            {
                TripId = tripId,
                UserId = user.UserId,
                RequestTime = TimeOnly.FromDateTime(DateTime.Now),
                RequestMessage = Message,
                RequestAddress = Address
            };

            await _requestService.CreateRequestAsync(request);
            TempData["SuccessMessage"] = "Anmodningen blev oprettet!";
            return RedirectToPage("/TripPages/AvailableTrips");

        }

    }
}
