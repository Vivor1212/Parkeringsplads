using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Parkeringsplads.Models;
using Parkeringsplads.Services.Interfaces;

namespace Parkeringsplads.Pages.TripPages
{
    public class TripDetailsModel : BasePageModel
    {
        private readonly ITripService _tripService;
        private readonly IRequestService _requestService;

        public TripDetailsModel(IUser userService, ITripService tripService, IRequestService requestService) : base(userService)
        {
            _tripService = tripService;
            _requestService = requestService;
        }

        public Trip Trip { get; set; }
        public IEnumerable<Car> DriverCars { get; set; } 

        private async Task<IActionResult> HandleValidationAndRedirect(UserValidation userResult, TripValidation tripResult = null)
        {
            if (!userResult.IsValid)
            {
                TempData["ErrorMessage"] = userResult.ErrorMessage;
                return RedirectToPage(userResult.RedirectPage);
            }

            if (tripResult != null && !tripResult.IsValid)
            {
                TempData["ErrorMessage"] = tripResult.ErrorMessage;
                return RedirectToPage(tripResult.RedirectPage);
            }

            return null;
        }

        public async Task<IActionResult> OnGetAsync(int tripId)
        {
            var userResult = await _userService.ValidateDriverAsync(HttpContext);
            var redirectResult = await HandleValidationAndRedirect(userResult);
            if (redirectResult != null)
            {
                return redirectResult;
            }

            var tripResult = await _tripService.GetDriverTripAsync(tripId, userResult.User.UserId);
            redirectResult = await HandleValidationAndRedirect(userResult, tripResult);
            if (redirectResult != null)
            {
                return redirectResult;
            }

            Trip = tripResult.Trip;
            DriverCars = await _tripService.GetDriversCarsAsync(Trip.Car.DriverId);
            return Page();
        }

        public async Task<IActionResult> OnPostAcceptRequestAsync(int tripId, int requestId)
        {
            var userResult = await _userService.ValidateDriverAsync(HttpContext);
            var redirectResult = await HandleValidationAndRedirect(userResult);
            if (redirectResult != null)
            {
                return redirectResult;
            }

            var tripResult = await _tripService.GetDriverTripAsync(tripId, userResult.User.UserId);
            redirectResult = await HandleValidationAndRedirect(userResult, tripResult);
            if (redirectResult != null)
            {
                return redirectResult;
            }

            var result = await _requestService.AcceptRequestAsync(requestId, tripId);
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
            return RedirectToPage(new { tripId });
        }

        public async Task<IActionResult> OnPostRejectRequestAsync(int tripId, int requestId)
        {
            var userResult = await _userService.ValidateDriverAsync(HttpContext);
            var redirectResult = await HandleValidationAndRedirect(userResult);
            if (redirectResult != null)
            {
                return redirectResult;
            }

            var tripResult = await _tripService.GetDriverTripAsync(tripId, userResult.User.UserId);
            redirectResult = await HandleValidationAndRedirect(userResult, tripResult);
            if (redirectResult != null)
            {
                return redirectResult;
            }

            var result = await _requestService.RejectRequestAsync(requestId);
            TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
            return RedirectToPage(new { tripId });
        }
    }
}
