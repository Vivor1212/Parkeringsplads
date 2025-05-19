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
    public class AddTripAdminModel : PageModel
    {
        private readonly ITripService _tripService;
        private readonly ParkeringspladsContext _context;

        public AddTripAdminModel(ITripService tripService, ParkeringspladsContext context)
        {
            _tripService = tripService;
            _context = context;
        }

        [BindProperty]
        public Trip Trip { get; set; } = new();

        [BindProperty]
        public string Direction { get; set; } = "ToSchool";

        [BindProperty]
        public int SelectedDriverId { get; set; }

        [BindProperty]
        public int SelectedCarId { get; set; }

        [BindProperty]
        public string SelectedAddress { get; set; } = "";

        [BindProperty]
        public string CustomAddress { get; set; } = "";

        [BindProperty]
        public bool UseCustomAddress { get; set; }

        [BindProperty]
        public int TripSeats { get; set; } = 1;

        public List<SelectListItem> Drivers { get; set; } = new();
        public List<SelectListItem> Cars { get; set; } = new();
        public List<string> UserAddresses { get; set; } = new();

        public string? DriverAddress { get; set; }

        [TempData]
        public string? SuccessMessage { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Set default date to today on GET
            if (Trip.TripDate == default)
            {
                Trip.TripDate = DateOnly.FromDateTime(DateTime.Today);
            }

            await LoadDriversAndCarsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await LoadDriversAndCarsAsync();

            // Validate the selected driver and car
            if (SelectedDriverId <= 0 || SelectedCarId <= 0)
            {
                ErrorMessage = "Vælg venligst både chauffør og bil.";
                return Page();
            }

            // Validate that the number of passengers is within the car's capacity
            var selectedCar = await _context.Car.FirstOrDefaultAsync(c => c.CarId == SelectedCarId);
            if (selectedCar != null && (TripSeats < 1 || TripSeats > selectedCar.CarCapacity))
            {
                ErrorMessage = $"Antal passagerer skal være mellem 1 og {selectedCar.CarCapacity}.";
                return Page();
            }

            // Validate the trip date
            if (Trip.TripDate < DateOnly.FromDateTime(DateTime.Today))
            {
                ErrorMessage = "Datoen for turen kan ikke være i fortiden.";
                return Page();
            }

            // Validate the address
            if (UseCustomAddress && string.IsNullOrWhiteSpace(CustomAddress))
            {
                ErrorMessage = "Du valgte en anden adresse, men udfyldte den ikke.";
                return Page();
            }

            var otherAddress = UseCustomAddress ? CustomAddress : SelectedAddress;
            if (string.IsNullOrWhiteSpace(otherAddress))
            {
                ErrorMessage = "Vælg eller angiv en adresse.";
                return Page();
            }

            if (string.IsNullOrWhiteSpace(DriverAddress))
            {
                ErrorMessage = "Kunne ikke hente chaufførens adresse.";
                return Page();
            }

            // Set trip directions based on the chosen direction
            if (Direction == "ToSchool")
            {
                Trip.FromDestination = DriverAddress;
                Trip.ToDestination = otherAddress;
            }
            else
            {
                Trip.FromDestination = otherAddress;
                Trip.ToDestination = DriverAddress;
            }

            // Set car ID and passengers count
            Trip.CarId = SelectedCarId;
            Trip.TripSeats = TripSeats;  // Store the number of seats for the trip

            // Create the trip
            await _tripService.CreateTripAsync(Trip);
            SuccessMessage = "Turen er oprettet!";
            return RedirectToPage();
        }

        private async Task LoadDriversAndCarsAsync()
        {
            Drivers = await _context.Driver
                .Include(d => d.User)
                .Select(d => new SelectListItem
                {
                    Value = d.DriverId.ToString(),
                    Text = $"{d.User.FirstName} {d.User.LastName} ({d.DriverLicense})"
                }).ToListAsync();

            if (SelectedDriverId > 0)
            {
                Cars = await _context.Car
                    .Where(c => c.DriverId == SelectedDriverId)
                    .Select(c => new SelectListItem
                    {
                        Value = c.CarId.ToString(),
                        Text = $"{c.CarModel} ({c.CarPlate})"
                    }).ToListAsync();

                var driver = await _context.Driver
                    .Include(d => d.User)
                        .ThenInclude(u => u.UserAddresses)
                            .ThenInclude(ua => ua.Address)
                    .FirstOrDefaultAsync(d => d.DriverId == SelectedDriverId);

                if (driver?.User?.UserAddresses != null)
                {
                    var firstAddress = driver.User.UserAddresses.FirstOrDefault()?.Address;
                    if (firstAddress != null)
                    {
                        DriverAddress = $"{firstAddress.AddressRoad} {firstAddress.AddressNumber}, {firstAddress.City?.PostalCode} {firstAddress.City?.CityName}";
                    }

                    UserAddresses = driver.User.UserAddresses
                        .Select(ua => $"{ua.Address.AddressRoad} {ua.Address.AddressNumber}, {ua.Address.City?.PostalCode} {ua.Address.City?.CityName}")
                        .ToList();
                }
            }

            UseCustomAddress = SelectedAddress == "__custom__";
        }
    }
}
