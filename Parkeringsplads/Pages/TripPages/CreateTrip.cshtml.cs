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
    public class CreateTripModel : BasePageModel
    {
        private readonly ITripService _tripService;

        public CreateTripModel(IUser userService, ITripService tripService) : base(userService)
        {
            _tripService = tripService;
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

            var driver = await _tripService.GetDriverWithCarsByEmailAsync(userEmail);
            if (driver == null || !driver.Cars.Any())
            {
                TempData["ErrorMessage"] = "Ingen biler fundet. Tilføj en bil <a href='/CarPages/Car'>her</a>.";
                return RedirectToPage("/Account/Profile");
            }

            var user = await _userService.GetUserWithDetailsByEmailAsync(userEmail);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Bruger ikke fundet.";
                return RedirectToPage("/Account/Login/Login");
            }

            UserAddresses = user.UserAddresses.Select(ua => ua.Address.FullAddress).ToList();
            SchoolAddress = user.School?.Address?.FullAddress ?? "Ukendt skoleadresse";

            Cars = driver.Cars.ToList();
            var selectedCar = Cars.FirstOrDefault(c => c.CarId == SelectedCarId) ?? Cars.FirstOrDefault();
            int carCapacity = selectedCar?.CarCapacity ?? 4;
            SeatOptions = Enumerable.Range(1, carCapacity).ToList();

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
            {
                return RedirectToPage("/Account/Login/Login");
            }

            var driver = await _tripService.GetDriverWithCarsByEmailAsync(userEmail);
            if (driver == null || !driver.Cars.Any())
            {
                TempData["ErrorMessage"] = "Ingen biler fundet. Tilføj en bil under Mine Biler.";
                return RedirectToPage("/Account/Profile");
            }

            var user = await _userService.GetUserWithDetailsByEmailAsync(userEmail);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Bruger ikke fundet.";
                return RedirectToPage("/Account/Login/Login");
            }

            UserAddresses = user.UserAddresses.Select(ua => ua.Address.FullAddress).ToList();
            SchoolAddress = user.School?.Address?.FullAddress ?? "Ukendt skoleadresse";
            Cars = driver.Cars.ToList();

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

            await _tripService.CreateTripAsync(Trip);
            TempData["SuccessMessage"] = "Turen blev oprettet!";
            return RedirectToPage("/TripPages/DriversTrips");
        }
    }
}
