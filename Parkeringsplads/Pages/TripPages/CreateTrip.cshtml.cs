using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Parkeringsplads.Models;
using Parkeringsplads.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Parkeringsplads.Pages.TripPages
{
    public class CreateTripModel : PageModel
    {
        private readonly ITripService _tripService;
        private readonly ParkeringspladsContext _context;

        public CreateTripModel(ITripService tripService, ParkeringspladsContext context)
        {
            _tripService = tripService;
            _context = context;
        }

        [BindProperty] public Trip Trip { get; set; } = new();

        [BindProperty(SupportsGet = true)] public string Direction { get; set; } = "Til Skole";
        [BindProperty(SupportsGet = true)] public int SelectedCarId { get; set; }
        [BindProperty(SupportsGet = true)] public string SelectedAddress { get; set; } = "";
        [BindProperty(SupportsGet = true)] public string CustomAddress { get; set; } = "";
        [BindProperty(SupportsGet = true)] public bool UseCustomAddress { get; set; }

        public List<Car> Cars { get; set; } = new();
        public List<string> UserAddresses { get; set; } = new();
        public List<int> SeatOptions { get; set; } = new();
        public string SchoolAddress { get; set; } = "";

        public async Task<IActionResult> OnGetAsync()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
                return RedirectToPage("/Account/Login/Login");

            var driver = await _context.Driver
                .Include(d => d.Cars)
                .FirstOrDefaultAsync(d => d.User.Email == userEmail);

            if (driver == null || !driver.Cars.Any())
            {
                TempData["ErrorMessage"] = "Ingen biler fundet. Tilføj en bil i din profil.";
                return RedirectToPage("/Account/Profile");
            }

            var user = await _context.User
                .Include(u => u.School).ThenInclude(s => s.Address).ThenInclude(a => a.City)
                .Include(u => u.UserAddresses).ThenInclude(ua => ua.Address).ThenInclude(a => a.City)
                .FirstOrDefaultAsync(u => u.Email == userEmail);

            UserAddresses = user.UserAddresses.Select(ua => ua.Address.FullAddress).ToList();
            SchoolAddress = user.School?.Address?.FullAddress ?? "Ukendt skoleadresse";

            var selectedCar = Cars.FirstOrDefault(c => c.CarId == SelectedCarId) ?? Cars.FirstOrDefault();
            int carCapacity = selectedCar?.CarCapacity ?? 4;
            SeatOptions = Enumerable.Range(1, carCapacity).ToList();

            Cars = driver.Cars.ToList();
            Trip.TripSeats = carCapacity;
            Trip.TripDate = DateOnly.FromDateTime(DateTime.Today);
            Trip.TripTime = new TimeOnly(DateTime.Now.Hour, 0);

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


            return Page();

        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
                return RedirectToPage("/Account/Login/Login");

            var driver = await _context.Driver
                .Include(d => d.Cars)
                .FirstOrDefaultAsync(d => d.User.Email == userEmail);

            if (driver == null || !driver.Cars.Any())
            {
                TempData["ErrorMessage"] = "Ingen biler fundet. Tilføj en bil i din profil.";
                return RedirectToPage("/TripPages/CreateTrip");
            }

            Cars = driver.Cars.ToList();

            var user = await _context.User
                .Include(u => u.School).ThenInclude(s => s.Address).ThenInclude(a => a.City)
                .Include(u => u.UserAddresses).ThenInclude(ua => ua.Address).ThenInclude(a => a.City)
                .FirstOrDefaultAsync(u => u.Email == userEmail);

            UserAddresses = user.UserAddresses.Select(ua => ua.Address.FullAddress).ToList();
            SchoolAddress = user.School?.Address?.FullAddress ?? "Ukendt skoleadresse";

            var car = Cars.FirstOrDefault(c => c.CarId == SelectedCarId);
            if (car == null)
            {
                TempData["ErrorMessage"] = "Ugyldig bil valgt.";
                return RedirectToPage("/TripPages/CreateTrip");
            }

            var address = UseCustomAddress ? CustomAddress?.Trim() : SelectedAddress;
            if (string.IsNullOrWhiteSpace(address))
                address = UserAddresses.FirstOrDefault() ?? "";

            Trip.CarId = SelectedCarId;
            Trip.TripSeats = car.CarCapacity;

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

            if (Trip.TripDate < DateOnly.FromDateTime(DateTime.Today))
            {
                TempData["ErrorMessage"] = "Datoen må ikke være i fortiden.";
                return RedirectToPage();
            }

            await _tripService.CreateTripAsync(Trip);
            TempData["SuccessMessage"] = "Turen blev oprettet!";
            return RedirectToPage("/TripPages/DriversTrips");
        }
    }
}
