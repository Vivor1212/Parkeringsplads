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

namespace Parkeringsplads.Pages.Admin
{
    public class AddRequestAdminModel : PageModel
    {
        private readonly IUser _userService;
        private readonly ITripService _tripService;
        private readonly IRequestService _requestService;

        public AddRequestAdminModel(IUser userService, ITripService tripService, IRequestService requestService)
        {
            _userService = userService;
            _tripService = tripService;
            _requestService = requestService;
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

            var isAdmin = HttpContext.Session.GetString("IsAdmin") == "true";
            if (!isAdmin)
            {
                return RedirectToPage("/Admin/NotAdmin");
            }

            await LoadDataAsync();

            if (SelectedUserId <= 0 || SelectedTripId <= 0)
            {
                TempData["ErrorMessage"] = "Vælg både bruger og tur.";
                return Page();
            }

            var user = await _userService.GetUserWithAddressesAsync(SelectedUserId);
            if (user?.UserAddresses.FirstOrDefault()?.Address is Address address)
            {
                Request.RequestAddress = $"{address.AddressRoad} {address.AddressNumber}, {address.City?.PostalCode} {address.City?.CityName}";
            }

            Request.UserId = SelectedUserId;
            Request.TripId = SelectedTripId;
            Request.RequestTime = TimeOnly.FromDateTime(DateTime.Now);
            Request.RequestStatus = null;

            await _requestService.CreateRequestAsync(Request);
            TempData["SuccessMessage"] = "Anmodningen er oprettet!";
            return RedirectToPage("/Admin/AdminDashboard");
        }

        private async Task LoadDataAsync()
        {
            Users = await _userService.UserDropDownAsync();
            TripList = await _tripService.GetTripsWithDetailsAsync();

            if (SelectedUserId > 0)
            {
                var user = await _userService.GetUserWithAddressesAsync(SelectedUserId);
                var addr = user?.UserAddresses.FirstOrDefault()?.Address;
                if (addr != null)
                {
                    UserAddress = $"{addr.AddressRoad} {addr.AddressNumber}, {addr.City?.PostalCode} {addr.City?.CityName}";
                }
            }
        }
    }
}
