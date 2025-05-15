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

        [BindProperty]
        public Trip Trip { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string Direction { get; set; } = "ToSchool";

        [BindProperty(SupportsGet = true)]
        public int SelectedCarId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SelectedAddress { get; set; } = "";

        [BindProperty(SupportsGet = true)]
        public string CustomAddress { get; set; } = "";

        [BindProperty(SupportsGet = true)]
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
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
                return RedirectToPage("/Account/Login");

            var driver = await _context.Driver
                .Include(d => d.Cars)
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.User.Email == userEmail);

            Cars = driver.Cars.ToList();

            var user = await _context.User
                .Include(u => u.School)
                    .ThenInclude(s => s.Address)
                        .ThenInclude(a => a.City)
                .Include(u => u.UserAddresses)
                    .ThenInclude(ua => ua.Address)
                        .ThenInclude(a => a.City)
                .FirstOrDefaultAsync(u => u.Email == userEmail);

            UserAddresses = user.UserAddresses
                .Select(ua => ua.Address.FullAddress)
                .ToList();

            SchoolAddress = user.School?.Address?.FullAddress ?? "Ukendt skoleadresse";

            // Sæt første adresse som default hvis ikke valgt endnu
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

        public async Task<IActionResult> OnPostAsync()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
                return RedirectToPage("/Account/Login");

            var driver = await _context.Driver
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.User.Email == userEmail);

            var user = await _context.User
                .Include(u => u.School)
                    .ThenInclude(s => s.Address)
                        .ThenInclude(a => a.City)
                .Include(u => u.UserAddresses)
                    .ThenInclude(ua => ua.Address)
                        .ThenInclude(a => a.City)
                .FirstOrDefaultAsync(u => u.Email == userEmail);

            SchoolAddress = user.School?.Address?.FullAddress ?? "Ukendt skoleadresse";

            string chosenAddress = UseCustomAddress
                ? CustomAddress?.Trim()
                : SelectedAddress ?? user.UserAddresses.FirstOrDefault()?.Address.FullAddress;

            if (string.IsNullOrWhiteSpace(chosenAddress))
            {
                ErrorMessage = "Adresse mangler. Vælg eller indtast en adresse.";
                return RedirectToPage(new
                {
                    Direction,
                    SelectedCarId,
                    SelectedAddress,
                    CustomAddress,
                    UseCustomAddress
                });
            }


            if (Direction == "FromSchool")
            {
                Trip.FromDestination = SchoolAddress;
                Trip.ToDestination = chosenAddress;
            }
            else
            {
                Trip.FromDestination = chosenAddress;
                Trip.ToDestination = SchoolAddress;
            }

            Trip.DriverId = driver.DriverId;

            await _tripService.CreateTripAsync(Trip);

            SuccessMessage = "Turen blev oprettet!";

            return RedirectToPage(new
            {
                Direction,
                SelectedCarId,
                SelectedAddress,
                CustomAddress,
                UseCustomAddress
            });
        }
    }
}
