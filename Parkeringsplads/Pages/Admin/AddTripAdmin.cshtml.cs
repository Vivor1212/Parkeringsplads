
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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

        [BindProperty] public int SelectedDriverId { get; set; }
        [BindProperty] public int SelectedCarId { get; set; }
        [BindProperty] public string Direction { get; set; } = "ToSchool";
        [BindProperty] public string SelectedAddress { get; set; } = "";
        [BindProperty] public string CustomAddress { get; set; } = "";
        [BindProperty] public bool UseCustomAddress { get; set; }
        [BindProperty] public Trip Trip { get; set; } = new();

        public Driver? Driver { get; set; }
        public List<Car> Cars { get; set; } = new();
        public List<string> UserAddresses { get; set; } = new();
        public List<int> SeatOptions { get; set; } = new();
        public string SchoolAddress { get; set; } = "";

        [TempData] public string? SuccessMessage { get; set; }
        [TempData] public string? ErrorMessage { get; set; }

        // Called when the page is first loaded (GET request)
        public async Task<IActionResult> OnGetAsync(int? selectedDriverId, int? selectedCarId, string? direction, string? selectedAddress, string? customAddress, bool? useCustomAddress)
        {
            if (selectedDriverId == null)
            {
                ErrorMessage = "Ingen chauffør valgt.";
                return RedirectToPage("SelectDriver");
            }

            SelectedDriverId = selectedDriverId.Value;
            SelectedCarId = selectedCarId ?? SelectedCarId;
            Direction = direction ?? Direction;
            SelectedAddress = selectedAddress ?? SelectedAddress;
            CustomAddress = customAddress ?? CustomAddress;
            UseCustomAddress = useCustomAddress ?? UseCustomAddress;

            return await LoadDataAndReturnPageAsync();
        }

        // Called when the form is submitted (POST request)
        public async Task<IActionResult> OnPostAsync()
        {
            return await SaveTripAsync();
        }

        // Loads data and prepares the page (form values, address, etc.)
        private async Task<IActionResult> LoadDataAndReturnPageAsync()
        {
            // Get the selected driver
            Driver = await _context.Driver
                .Include(d => d.User)
                .Include(d => d.Cars)
                .FirstOrDefaultAsync(d => d.DriverId == SelectedDriverId);

            if (Driver == null)
            {
                ErrorMessage = "Chaufføren blev ikke fundet.";
                return Page();
            }

            // Get the cars for the selected driver
            Cars = Driver.Cars.ToList();

            var user = await _context.User
                .Include(u => u.School).ThenInclude(s => s.Address).ThenInclude(a => a.City)
                .Include(u => u.UserAddresses).ThenInclude(ua => ua.Address).ThenInclude(a => a.City)
                .FirstOrDefaultAsync(u => u.UserId == Driver.UserId);

            if (user == null)
            {
                ErrorMessage = "Brugeren blev ikke fundet.";
                return Page();
            }

            // Collect user addresses
            UserAddresses = user.UserAddresses.Select(ua => ua.Address.FullAddress).ToList();
            SchoolAddress = user.School?.Address?.FullAddress ?? "Ukendt skoleadresse";

            // Get the selected car's capacity
            var selectedCar = Cars.FirstOrDefault(c => c.CarId == SelectedCarId) ?? Cars.FirstOrDefault();
            int carCapacity = selectedCar?.CarCapacity ?? 4;
            SeatOptions = Enumerable.Range(1, carCapacity).ToList();

            // Set the From/To destinations based on the direction
            var address = UseCustomAddress ? CustomAddress?.Trim() : SelectedAddress;
            if (string.IsNullOrWhiteSpace(address))
                address = UserAddresses.FirstOrDefault() ?? "";

            if (Direction == "FromSchool")
            {
                Trip.FromDestination = SchoolAddress;
                Trip.ToDestination = address;
            }
            else
            {
                Trip.FromDestination = address;
                Trip.ToDestination = SchoolAddress;
            }

            // Set default trip date and time if they are not already set
            if (Trip.TripDate == default)
                Trip.TripDate = DateOnly.FromDateTime(DateTime.Today);

            if (Trip.TripTime == default)
                Trip.TripTime = new TimeOnly(DateTime.Now.Hour, 0);

            return Page();
        }

        // Saves the trip details
        private async Task<IActionResult> SaveTripAsync()
        {
            // Ensure the model state is valid before proceeding
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Der er en fejl i formularen.";

                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Error: {error.ErrorMessage}");
                }

                return Page();  // Return the page with errors
            }

            // Ensure a valid car is selected
            var car = Cars.FirstOrDefault(c => c.CarId == SelectedCarId);
            if (car == null)
            {
                ErrorMessage = "Ugyldig bil valgt.";
                return Page();
            }

            // Set trip details based on the form input and car capacity
            Trip.CarId = car.CarId;
            Trip.TripSeats = Trip.TripSeats > 0 ? Trip.TripSeats : car.CarCapacity;

            // Validate the trip date
            if (Trip.TripDate < DateOnly.FromDateTime(DateTime.Today))
            {
                ErrorMessage = "Datoen må ikke være i fortiden.";
                return Page();
            }

            // Log trip details for debugging
            Console.WriteLine($"Trip: {Trip.FromDestination} -> {Trip.ToDestination}, Date: {Trip.TripDate}, Time: {Trip.TripTime}, Seats: {Trip.TripSeats}, CarId: {Trip.CarId}");

            // Save the trip
            await _tripService.CreateTripAsync(Trip);
            SuccessMessage = "Turen blev oprettet!";

            // Redirect to the same page with success message
            return RedirectToPage();
        }
    }
}
