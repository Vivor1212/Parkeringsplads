using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;
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

        [BindProperty]
        public Trip Trip { get; set; } = new();

        [BindProperty]
        public string Direction { get; set; } = "ToSchool";

        [BindProperty]
        public int SelectedCarId { get; set; }

        [BindProperty]
        public string SelectedAddress { get; set; } = "";

        [BindProperty]
        public string CustomAddress { get; set; } = "";

        [BindProperty]
        public bool UseCustomAddress { get; set; }

        [TempData]
        public string? SuccessMessage { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public List<Car> Cars { get; set; } = new();
        public List<string> UserAddresses { get; set; } = new();
        public List<int> SeatOptions { get; set; } = new();
        public string SchoolAddress { get; set; } = "";

        public async Task<IActionResult> OnGetAsync()
        {
            return await LoadPageData();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
                return RedirectToPage("/Account/Login");

            var driver = await _context.Driver
                .Include(d => d.User)
                .Include(d => d.Cars)
                .FirstOrDefaultAsync(d => d.User.Email == userEmail);

            if (driver == null)
            {
                ErrorMessage = "Driver not found.";
                return RedirectToPage("/Account/Login");
            }

            var user = await _context.User
                .Include(u => u.School)
                    .ThenInclude(s => s.Address)
                        .ThenInclude(a => a.City)
                .Include(u => u.UserAddresses)
                    .ThenInclude(ua => ua.Address)
                        .ThenInclude(a => a.City)
                .FirstOrDefaultAsync(u => u.Email == userEmail);

            if (user == null)
            {
                ErrorMessage = "User not found.";
                return RedirectToPage("/Account/Login");
            }

            Cars = driver.Cars.ToList();
            UserAddresses = user.UserAddresses.Select(ua => ua.Address.FullAddress).ToList();
            SchoolAddress = user.School?.Address?.FullAddress ?? "Ukendt skoleadresse";

            if (!Cars.Any())
            {
                ErrorMessage = "Ingen biler fundet. Tilføj en bil i din profil.";
                return RedirectToPage("/Account/Profile");
            }

            if (string.IsNullOrWhiteSpace(SelectedAddress) && !UseCustomAddress)
            {
                SelectedAddress = UserAddresses.FirstOrDefault();
            }

            var selectedCar = Cars.FirstOrDefault(c => c.CarId == SelectedCarId) ?? Cars.FirstOrDefault();
            int carCapacity = selectedCar?.CarCapacity ?? 4;
            SeatOptions = Enumerable.Range(1, carCapacity).ToList();

            string finalAddress = UseCustomAddress ? CustomAddress?.Trim() : SelectedAddress;
            if (Direction == "FromSchool")
            {
                Trip.FromDestination = SchoolAddress;
                Trip.ToDestination = finalAddress;
            }
            else
            {
                Trip.FromDestination = finalAddress;
                Trip.ToDestination = SchoolAddress;
            }

            if (SelectedCarId <= 0)
            {
                ErrorMessage = "Ingen bil valgt. Vælg en gyldig bil";
                return RedirectToPage(new {Direction, SelectedCarId, SelectedAddress, CustomAddress, UseCustomAddress});
            }

            var car = await _context.Car.FirstOrDefaultAsync(c => c.CarId == SelectedCarId && c.DriverId == driver.DriverId);

            if (car == null)
            {
                ErrorMessage = $"Ugyldig bil valgt. CarId: {SelectedCarId} findes ikke eller tilhører ikke føreren.";
                return Page();
            }

            if (string.IsNullOrWhiteSpace(Trip.FromDestination) || string.IsNullOrWhiteSpace(Trip.ToDestination))
            {
                ErrorMessage = "Udfyld venligst både fra- og til destination.";
                return Page();
            }

            Trip.CarId = SelectedCarId;

            try
            {
                await _tripService.CreateTripAsync(Trip);
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlEx && sqlEx.Number == 547) // Foreign key violation
            {
                ErrorMessage = $"Fejl ved oprettelse af tur: Ugyldigt CarId ({SelectedCarId}). Sørg for, at den valgte bil findes.";
                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Fejl ved oprettelse af tur: {ex.Message}";
                return Page();
            }

            SuccessMessage = "Turen blev oprettet!";
            return RedirectToPage();
        }

        private async Task<IActionResult> LoadPageData()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
                return RedirectToPage("/Account/Login");

            var driver = await _context.Driver
                .Include(d => d.User)
                .Include(d => d.Cars)
                .FirstOrDefaultAsync(d => d.User.Email == userEmail);

            if (driver == null)
            {
                ErrorMessage = "Driver not found.";
                return RedirectToPage("/Account/Login");
            }

            var user = await _context.User
                .Include(u => u.School)
                    .ThenInclude(s => s.Address)
                        .ThenInclude(a => a.City)
                .Include(u => u.UserAddresses)
                    .ThenInclude(ua => ua.Address)
                        .ThenInclude(a => a.City)
                .FirstOrDefaultAsync(u => u.Email == userEmail);

            if (user == null)
            {
                ErrorMessage = "User not found.";
                return RedirectToPage("/Account/Login");
            }

            Cars = driver.Cars.ToList();
            UserAddresses = user.UserAddresses
                .Select(ua => ua.Address.FullAddress)
                .ToList();
            SchoolAddress = user.School?.Address?.FullAddress ?? "Ukendt skoleadresse";

            if (!Cars.Any())
            {
                ErrorMessage = "Ingen biler fundet. Tilføj en bil i din profil.";
                return RedirectToPage("/Account/Profile");
            }

            if (string.IsNullOrWhiteSpace(SelectedAddress) && !UseCustomAddress)
            {
                SelectedAddress = UserAddresses.FirstOrDefault();
            }

            var selectedCar = Cars.FirstOrDefault(c => c.CarId == SelectedCarId) ?? Cars.FirstOrDefault();
            int carCapacity = selectedCar?.CarCapacity ?? 4;
            SeatOptions = Enumerable.Range(1, carCapacity).ToList();

            Trip = new Trip
            {
                TripDate = DateOnly.FromDateTime(DateTime.Today),
                TripSeats = carCapacity
            };

            string finalAddress = UseCustomAddress ? CustomAddress?.Trim() : SelectedAddress;
            if (Direction == "FromSchool")
            {
                Trip.FromDestination = SchoolAddress;
                Trip.ToDestination = finalAddress;
            }
            else
            {
                Trip.FromDestination = finalAddress;
                Trip.ToDestination = SchoolAddress;
            }

            return Page();
        }
    }
}
