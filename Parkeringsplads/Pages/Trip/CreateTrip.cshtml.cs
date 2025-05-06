using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Parkeringsplads.Models;
using Parkeringsplads.Services.Interfaces;

namespace Parkeringsplads.Pages.Trip
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

        #region BindProperty
        [BindProperty]
        public Parkeringsplads.Models.Trip Trip { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Direction { get; set; } = "ToSchool";

        [BindProperty(SupportsGet = true)]
        public int SelectedCarId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SelectedAddress { get; set; }

        [BindProperty(SupportsGet = true)]
        public string CustomAddress { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool UseCustomAddress { get; set; }
        #endregion

        public List<Car> Cars { get; set; } = new();
        public List<string> UserAddresses { get; set; } = new();
        public List<int> SeatOptions { get; set; } = new();
        public string SchoolAddress { get; set; }

        public IActionResult OnGet()
        {
            var userEmail = "Syhlerp@yahoo.dk";

            var driver = _context.Drivers
                .Include(d => d.Cars)
                .Include(d => d.User)
                .FirstOrDefault(d => d.User.Email == userEmail);

            Cars = driver.Cars.ToList();

            var user = _context.Users
                .Include(u => u.School)
                    .ThenInclude(s => s.Address)
                        .ThenInclude(a => a.City)
                .Include(u => u.Addresses)
                    .ThenInclude(a => a.City)
                .FirstOrDefault(u => u.Email == userEmail);

            UserAddresses = user.Addresses
                .Select(a => a.FullAddress)
                .ToList();

            SchoolAddress = user.School?.Address?.FullAddress ?? "Ukendt skoleadresse";

            var selectedCar = Cars.FirstOrDefault(c => c.CarId == SelectedCarId) ?? Cars.FirstOrDefault();
            int carCapacity = selectedCar?.CarCapacity ?? 4;

            SeatOptions = Enumerable.Range(1, carCapacity).ToList();

            Trip = new Parkeringsplads.Models.Trip
            {
                TripDate = DateOnly.FromDateTime(DateTime.Today),
                TripTime = TimeOnly.FromDateTime(DateTime.Now.AddHours(1)),
                TripSeats = carCapacity
            };

            // Brug adressevalg
            string finalAddress = UseCustomAddress
                ? CustomAddress?.Trim()
                : SelectedAddress ?? UserAddresses.FirstOrDefault();

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

        public IActionResult OnPost()
        {
            var userEmail = "Syhlerp@yahoo.dk";

            var driver = _context.Drivers
                .Include(d => d.User)
                .FirstOrDefault(d => d.User.Email == userEmail);

            var user = _context.Users
                .Include(u => u.School)
                    .ThenInclude(s => s.Address)
                        .ThenInclude(a => a.City)
                .Include(u => u.Addresses)
                    .ThenInclude(a => a.City)
                .FirstOrDefault(u => u.Email == userEmail);

            SchoolAddress = user.School?.Address?.FullAddress ?? "Ukendt skoleadresse";

            string chosenAddress = UseCustomAddress
                ? CustomAddress?.Trim()
                : SelectedAddress ?? user.Addresses.FirstOrDefault()?.FullAddress;

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

            _tripService.CreateTrip(Trip);

            return RedirectToPage("/Index");
        }
    }
}
