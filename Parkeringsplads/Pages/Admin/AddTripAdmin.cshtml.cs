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

        [BindProperty] public Trip Trip { get; set; } = new();
        [BindProperty] public string Direction { get; set; } = "ToSchool";
        [BindProperty] public int SelectedDriverId { get; set; }
        [BindProperty] public int SelectedCarId { get; set; }
        [BindProperty] public string SelectedAddress { get; set; } = "";
        [BindProperty] public string CustomAddress { get; set; } = "";
        [BindProperty] public bool UseCustomAddress { get; set; }
        [BindProperty] public int TripSeats { get; set; } = 1;

        public List<SelectListItem> Drivers { get; set; } = new();
        public List<SelectListItem> Cars { get; set; } = new();
        public List<string> UserAddresses { get; set; } = new();

        public string? DriverAddress { get; set; }
        public string? SchoolAddress { get; set; }

        [TempData] public string? SuccessMessage { get; set; }
        [TempData] public string? ErrorMessage { get; set; }

        public Dictionary<int, int> CarCapacities { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? selectedDriverId)
        {
            var isAdmin = HttpContext.Session.GetString("IsAdmin");
            if (string.IsNullOrEmpty(isAdmin) || isAdmin != "true")
                return RedirectToPage("/Admin/NotAdmin");

            if (Trip.TripDate == default)
                Trip.TripDate = DateOnly.FromDateTime(DateTime.Today);

            if (Trip.TripTime == default)
                Trip.TripTime = new TimeOnly(7, 0);

            // Pass selectedDriverId to LoadDriversAndCarsAsync
            await LoadDriversAndCarsAsync(selectedDriverId);

            return Page();
        }


        public async Task<IActionResult> OnPostAsync()
        {
            // Ensure the selected driver is available
            await LoadDriversAndCarsAsync(SelectedDriverId);

            if (SelectedDriverId <= 0 || SelectedCarId <= 0)
            {
                ErrorMessage = "Vælg venligst både chauffør og bil.";
                return Page();  // Return to the same page in case of error
            }

            var selectedCar = await _context.Car.FirstOrDefaultAsync(c => c.CarId == SelectedCarId);
            if (selectedCar != null && (TripSeats < 1 || TripSeats > selectedCar.CarCapacity))
            {
                ErrorMessage = $"Antal passagerer skal være mellem 1 og {selectedCar.CarCapacity}.";
                return Page();  // Return to the same page in case of error
            }

            if (Trip.TripDate < DateOnly.FromDateTime(DateTime.Today))
            {
                ErrorMessage = "Datoen for turen kan ikke være i fortiden.";
                return Page();  // Return to the same page in case of error
            }

            if (Trip.TripTime == default)
            {
                ErrorMessage = "Tidspunkt for turen skal angives.";
                return Page();  // Return to the same page in case of error
            }

            if (UseCustomAddress && string.IsNullOrWhiteSpace(CustomAddress))
            {
                ErrorMessage = "Du valgte en anden adresse, men udfyldte den ikke.";
                return Page();  // Return to the same page in case of error
            }

            string otherAddress = UseCustomAddress ? CustomAddress : SelectedAddress;

            if (string.IsNullOrWhiteSpace(otherAddress))
            {
                ErrorMessage = "Vælg eller angiv en adresse.";
                return Page();  // Return to the same page in case of error
            }

            if (string.IsNullOrWhiteSpace(DriverAddress) || string.IsNullOrWhiteSpace(SchoolAddress))
            {
                ErrorMessage = "Kunne ikke hente nødvendige adresser.";
                return Page();  // Return to the same page in case of error
            }

            Trip.CarId = SelectedCarId;
            Trip.TripSeats = TripSeats;

            // Ensure correct address assignments based on the direction
            if (Direction == "ToSchool")
            {
                Trip.FromDestination = UseCustomAddress ? CustomAddress : DriverAddress; // Use driver address as the "from" address
                Trip.ToDestination = SchoolAddress; // To the school address
            }
            else
            {
                Trip.FromDestination = SchoolAddress; // Start from the school address
                Trip.ToDestination = UseCustomAddress ? CustomAddress : DriverAddress; // Use driver address as the "to" address
            }

            await _tripService.CreateTripAsync(Trip);
            SuccessMessage = "Turen er oprettet!";
            return RedirectToPage("/Admin/AdminDashboard");  // Redirect to the dashboard after success
        }




        private async Task LoadDriversAndCarsAsync(int? selectedDriverId)
        {
            // Load School Address
            var school = await _context.School
                .Include(s => s.Address)
                    .ThenInclude(a => a.City)
                .FirstOrDefaultAsync();

            if (school?.Address != null)
            {
                SchoolAddress = $"{school.Address.AddressRoad} {school.Address.AddressNumber}, {school.Address.City?.PostalCode} {school.Address.City?.CityName}";
            }

            // Load all Drivers
            Drivers = await _context.Driver
                .Include(d => d.User)
                .Select(d => new SelectListItem
                {
                    Value = d.DriverId.ToString(),
                    Text = $"{d.User.FirstName} {d.User.LastName} ({d.DriverLicense})"
                }).ToListAsync();

            // Clear previous car-related data
            CarCapacities.Clear();
            Cars.Clear();
            UserAddresses.Clear();

            if (selectedDriverId.HasValue && selectedDriverId > 0)
            {
                // Load Cars for the selected driver only
                var cars = await _context.Car
                    .Where(c => c.DriverId == selectedDriverId)
                    .ToListAsync();

                CarCapacities = cars.ToDictionary(c => c.CarId, c => c.CarCapacity);
                Cars = cars.Select(c => new SelectListItem
                {
                    Value = c.CarId.ToString(),
                    Text = $"{c.CarModel} ({c.CarPlate})"
                }).ToList();

                // Load driver addresses
                var driver = await _context.Driver
                    .Include(d => d.User)
                        .ThenInclude(u => u.UserAddresses)
                            .ThenInclude(ua => ua.Address)
                                .ThenInclude(a => a.City)
                    .FirstOrDefaultAsync(d => d.DriverId == selectedDriverId);

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

            // Use custom address if selected
            UseCustomAddress = SelectedAddress == "__custom__";
        }


    }
}
