
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

        [BindProperty] public Trip Trip { get; set; } = new();
        [BindProperty(SupportsGet = true)] public string Direction { get; set; } = "Til Skole";
        [BindProperty(SupportsGet = true)] public int SelectedCarId { get; set; }
        [BindProperty(SupportsGet = true)] public string SelectedAddress { get; set; } = "";
        [BindProperty(SupportsGet = true)] public string CustomAddress { get; set; } = "";
        [BindProperty(SupportsGet = true)] public bool UseCustomAddress { get; set; }
        [BindProperty(SupportsGet = true)] public int selectedDriverId { get; set; }

        public List<Car> Cars { get; set; } = new();
        public List<string> UserAddresses { get; set; } = new();
        public List<int> SeatOptions { get; set; } = new();
        public string SchoolAddress { get; set; } = "";

        public async Task<IActionResult> OnGetAsync()
        {

            var isAdmin = HttpContext.Session.GetString("IsAdmin");

            if (string.IsNullOrEmpty(isAdmin) || isAdmin != "true")
            {
                return RedirectToPage("/Admin/NotAdmin");
            }

            if (selectedDriverId <= 0)
            {
                TempData["ErrorMessage"] = "Ingen chauffør valgt. Vælg en chauffør først.";
                return RedirectToPage("/Admin/ChooseDriverAdmin");
            }

            var driver = await _context.Driver
                .Include(d => d.Cars)
                .Include(d => d.User)
                .ThenInclude(u => u.School).ThenInclude(s => s.Address).ThenInclude(a => a.City)
                .Include(d => d.User)
                .ThenInclude(u => u.UserAddresses).ThenInclude(ua => ua.Address).ThenInclude(a => a.City)
                .FirstOrDefaultAsync(d => d.DriverId == selectedDriverId);

            if (driver == null || !driver.Cars.Any())
            {
                TempData["ErrorMessage"] = "Ingen biler fundet for den valgte chauffør.";
                return RedirectToPage("/Admin/AdminDashboard");
            }

            UserAddresses = driver.User.UserAddresses.Select(ua => ua.Address.FullAddress).ToList();
            SchoolAddress = driver.User.School?.Address?.FullAddress ?? "Ukendt skoleadresse";

            var selectedCar = driver.Cars.FirstOrDefault(c => c.CarId == SelectedCarId) ?? driver.Cars.FirstOrDefault();
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
            if (selectedDriverId <= 0)
            {
                TempData["ErrorMessage"] = "Ingen chauffør valgt.";
                return RedirectToPage("/Admin/ChooseDriverAdmin");
            }

            var driver = await _context.Driver
                .Include(d => d.Cars)
                .Include(d => d.User)
                .ThenInclude(u => u.School).ThenInclude(s => s.Address).ThenInclude(a => a.City)
                .Include(d => d.User)
                .ThenInclude(u => u.UserAddresses).ThenInclude(ua => ua.Address).ThenInclude(a => a.City)
                .FirstOrDefaultAsync(d => d.DriverId == selectedDriverId);

            if (driver == null || !driver.Cars.Any())
            {
                TempData["ErrorMessage"] = "Ingen biler fundet for den valgte chauffør.";
                return RedirectToPage("/Admin/AddTripAdmin");
            }

            Cars = driver.Cars.ToList();

            UserAddresses = driver.User.UserAddresses.Select(ua => ua.Address.FullAddress).ToList();
            SchoolAddress = driver.User.School?.Address?.FullAddress ?? "Ukendt skoleadresse";

            var car = Cars.FirstOrDefault(c => c.CarId == SelectedCarId);
            if (car == null)
            {
                TempData["ErrorMessage"] = "Ugyldig bil valgt.";
                return RedirectToPage("/Admin/AddTripAdmin");
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
            return RedirectToPage("/Admin/AdminDashboard");
        }
    }
}
    
