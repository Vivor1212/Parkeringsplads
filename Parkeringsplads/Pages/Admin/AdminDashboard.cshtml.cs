using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Parkeringsplads.Models;
using Parkeringsplads.Services.EFServices;
using Parkeringsplads.Services.Interfaces;

namespace Parkeringsplads.Pages.Admin
{
    public class AdminDashboardModel : PageModel
    {
        private readonly ICarService _carService;
        private readonly IUser _userService;
        private readonly ICityService _cityService;
        private readonly IAddressService _addressService;
        private readonly IDriverService _driverService;
        private readonly IRequestService _requestService;
        private readonly ITripService _tripService;
        private readonly ISchoolService _schoolService;


        public AdminDashboardModel(ICarService carService, IUser userService, ICityService cityService, IAddressService addressService, IDriverService driverService, IRequestService requestService, ITripService tripService, ISchoolService schoolService)
        {
            _carService = carService;
            _userService = userService;
            _cityService = cityService;
            _driverService = driverService;
            _requestService = requestService;
            _tripService = tripService;
            _schoolService = schoolService;
        }

        //User Search
        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }
        public List<User> Users { get; set; }

        //Car Search
        [BindProperty(SupportsGet = true)]
        public string CarSearchTerm { get; set; }
        public List<Car> Cars { get; set; }

        //City search
        [BindProperty(SupportsGet = true)]
        public string CitySearchTerm { get; set; }
        public List<City> Cities { get; set; }

        //Address Search
        [BindProperty(SupportsGet = true)]
        public string AddressSearchTerm { get; set; }
        public List<Address> Addresses { get; set; }

        //School Search
        [BindProperty(SupportsGet = true)]
        public string SchoolSearchTerm { get; set; }
        public List<School> Schools { get; set; }

        //Driver Search
        [BindProperty(SupportsGet = true)]
        public string DriverSearchTerm { get; set; }
        public List<Driver> Drivers { get; set; }

        //Trip Search
        [BindProperty(SupportsGet = true)]
        public string TripSearchTerm { get; set; }
        public List<Trip> Trips { get; set; }

        //Request Search
        [BindProperty(SupportsGet = true)]
        public string RequestSearchTerm { get; set; }
        public List<Request> Requests { get; set; }
        public Dictionary<int, bool> AddressUsageMap { get; set; } = new Dictionary<int, bool>();



        public async Task<IActionResult> OnGetAsync()
        {
            var isAdmin = HttpContext.Session.GetString("IsAdmin") == "true";
            if (!isAdmin)
            {
                return RedirectToPage("/Admin/NotAdmin");
            }

            Users = await _userService.GetUsersWithSchoolAsync(SearchTerm);
            Cars = await _carService.GetCarsAsync(CarSearchTerm);
            Cities = await _cityService.GetCitiesAsync(CitySearchTerm);
            Addresses = await _addressService.GetAddressesWithCityAsync(AddressSearchTerm);
            Schools = await _schoolService.GetSchoolsWithAddressAsync(SchoolSearchTerm);
            Drivers = await _driverService.GetDriversWithUserAsync(DriverSearchTerm);
            Trips = await _tripService.GetTripsWithDriverAsync(TripSearchTerm);
            Requests = await _requestService.GetRequestsWithDetailsAsync(RequestSearchTerm);

            AddressUsageMap = new Dictionary<int, bool>();
            foreach (var address in Addresses)
            {
                bool isInUse = await _addressService.IsAddressInUseAsync(address.AddressId);
                AddressUsageMap[address.AddressId] = isInUse;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteCarAsync(int carId)
        {
            try
            {
                await _carService.DeleteCarAsync(carId);

                TempData["SuccessMessage"] = "Bil slettet.";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Fejl ved sletning af bil: " + ex.Message;
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnPostDeleteUserAsync(int userId)
        {
            var result = await _userService.DeleteUserAsync(userId);

            if (result)
            {
                TempData["SuccessMessage"] = "Bruger slettet.";
                return RedirectToPage();
            }
            else
            {
                TempData["ErrorMessage"] = "Fejl ved sletning af bruger.";
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnPostDeleteCityAsync(int cityId)
        {
            try
            {
                await _cityService.DeleteCityAsync(cityId);
                TempData["SuccessMessage"] = "By slettet.";
                return RedirectToPage("/Admin/admindashboard");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Fejl ved sletning af by: " + ex.Message;
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnPostDeleteDriverAsync(int driverId)
        {
            try
            {
                await _driverService.DeleteDriverAsync(driverId);
                TempData["SuccessMessage"] = "Chauffør slettet.";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Fejl ved sletning af chauffør: " + ex.Message;
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnPostDeleteRequestAsync(int requestId)
        {
            try
            {
                await _requestService.DeleteRequestAsync(requestId);
                TempData["SuccessMessage"] = "Adnmodning slettet.";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Fejl ved sletning af anmodning: " + ex.Message;
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnPostDeleteTripAsync(int tripId)
        {
            try
            {
                await _tripService.AdminDeleteTripAsync(tripId);
                TempData["SuccessMessage"] = "Tur slettet.";
                return RedirectToPage();
            }
            catch (ArgumentException ex)
            {
                TempData["ErrorMessage"] = "Fejl ved sletning af tur: " + ex.Message;
                return NotFound(ex.Message);
            }
        }

        public async Task<IActionResult> OnPostDeleteSchoolAsync(int schoolId)
        {
            try
            {
                await _schoolService.DeleteSchoolAsync(schoolId);
                TempData["SuccessMessage"] = "Skole slettet.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Fejl ved sletning af skole: {ex.Message}";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAddressAsync(int id)
        {
            try
            {
                var deleted = await _addressService.DeleteAddressAsync(id);

                if (!deleted)
                {
                    TempData["ErrorMessage"] = "Adresse kunne ikke slettes, da den er i brug.";
                }
                else
                {
                    TempData["SuccessMessage"] = "Adressen er blevet slettet.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Fejl ved sletning af adresse: {ex.Message}";
            }

            return RedirectToPage();
        }
    }
}