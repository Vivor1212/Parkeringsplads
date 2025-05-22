using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Parkeringsplads.Models;
using Parkeringsplads.Services.EFServices;
using Parkeringsplads.Services.Interfaces;

namespace Parkeringsplads.Pages.TripPages
{
    public class DriversTripsModel : BasePageModel
    {
        private readonly ITripService _tripService;

        public DriversTripsModel(IUser userService, ITripService tripService) : base(userService)
        {
            _tripService = tripService;
        }

        public IEnumerable<Trip> Trips { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userResult = await _userService.ValidateDriverAsync(HttpContext);
            var redirectResult = await HandleValidationAndRedirect(userResult);
            if (redirectResult != null)
            {
                return redirectResult;
            }

            Trips = await _tripService.GetDriversFutureTripsAsync(userResult.User.UserId);
            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int tripId)
        {
            var userResult = await _userService.ValidateDriverAsync(HttpContext);
            var redirectResult = await HandleValidationAndRedirect(userResult);
            if (redirectResult != null)
            {
                return redirectResult;
            }

            var result = await _tripService.DeleteTripAsync(tripId, userResult.User.UserId);
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
            return RedirectToPage();
        }
    }
}
