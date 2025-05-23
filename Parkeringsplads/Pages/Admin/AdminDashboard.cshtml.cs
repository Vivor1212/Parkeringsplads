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
        private readonly ParkeringspladsContext _context;
        private readonly ICarService _carService;
        private readonly IUser _userService;
        private readonly ICityService _cityService;
        private readonly IDriverService _driverService;
        private readonly IRequestService _requestService;
        private readonly ITripService _tripService;
        private readonly ISchoolService _schoolService;
        private readonly IAddressService _addressService;

        public AdminDashboardModel(ParkeringspladsContext context, ICarService carService, IUser userService, ICityService cityService, IDriverService driverService, IRequestService requestService, ITripService tripService, ISchoolService schoolService, IAddressService addressService)
        {
            _context = context;
            _carService = carService;
            _userService = userService;
            _cityService = cityService;
            _driverService = driverService;
            _requestService = requestService;
            _tripService = tripService;
            _schoolService = schoolService;
            _addressService = addressService;
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

            var isAdmin = HttpContext.Session.GetString("IsAdmin");

            if (string.IsNullOrEmpty(isAdmin) || isAdmin != "true")
            {
                return RedirectToPage("/Admin/NotAdmin");
            }

            //Usersøgning

            var query = _context.User.Include(u => u.School).AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                var lowerTerm = SearchTerm.ToLower();

                query = query.Where(u =>
                    u.FirstName.ToLower().Contains(lowerTerm) ||
                    u.LastName.ToLower().Contains(lowerTerm) ||
                    u.Email.ToLower().Contains(lowerTerm) ||
                    u.Phone.ToLower().Contains(lowerTerm) ||
                    u.Title.ToLower().Contains(lowerTerm) ||
                    u.SchoolId.ToString().Contains(lowerTerm) ||
                    (u.School != null && u.School.SchoolName.ToLower().Contains(lowerTerm))
                );
            }

            Users = await query.ToListAsync();

            //Carsøgning
            var carQuery = _context.Car.AsQueryable();

            if (!string.IsNullOrWhiteSpace(CarSearchTerm))
            {
                var lower = CarSearchTerm.ToLower();
                carQuery = carQuery.Where(c =>
                    c.CarModel.ToLower().Contains(lower) ||
                    c.CarPlate.ToLower().Contains(lower) ||
                    c.CarCapacity.ToString().Contains(lower) ||
                    c.DriverId.ToString().Contains(lower)
                );
            }

            Cars = await carQuery.ToListAsync();

            //Citysøgning
            var cityQuery = _context.City.AsQueryable();

            if (!string.IsNullOrWhiteSpace(CitySearchTerm))
            {
                var lower = CitySearchTerm.ToLower();
                cityQuery = cityQuery.Where(c =>
                    c.CityName.ToLower().Contains(lower) ||
                    c.PostalCode.Contains(lower)
                );
            }

            Cities = await cityQuery.ToListAsync();

            //Adressesøgning
            var addressQuery = _context.Address
                .Include(a => a.City)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(AddressSearchTerm))
            {
                var lowerAddressSearchTerm = AddressSearchTerm.ToLower();
                addressQuery = addressQuery.Where(a =>
                    a.AddressRoad.ToLower().Contains(lowerAddressSearchTerm) ||
                    a.AddressNumber.ToLower().Contains(lowerAddressSearchTerm) ||
                    a.City.CityName.ToLower().Contains(lowerAddressSearchTerm) ||
                    a.City.PostalCode.Contains(lowerAddressSearchTerm)
                );
            }
            Addresses = await addressQuery.ToListAsync();

            Addresses = await addressQuery.ToListAsync();

            AddressUsageMap = new Dictionary<int, bool>();

            foreach (var address in Addresses)
            {
                bool inUse = await IsAddressInUseAsync(address.AddressId);
                AddressUsageMap[address.AddressId] = inUse;
            }


            //Schoolsøgning
            var schoolQuery = _context.School
                .Include(s => s.Address)
                .ThenInclude(a => a.City)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(SchoolSearchTerm))
            {
                var lowerSchoolSearchTerm = SchoolSearchTerm.ToLower();
                schoolQuery = schoolQuery.Where(s =>
                    s.SchoolName.ToLower().Contains(lowerSchoolSearchTerm) ||
                    s.Address.AddressRoad.ToLower().Contains(lowerSchoolSearchTerm) ||
                    s.Address.AddressNumber.ToLower().Contains(lowerSchoolSearchTerm) ||
                    s.Address.City.CityName.ToLower().Contains(lowerSchoolSearchTerm) ||
                    s.Address.City.PostalCode.Contains(lowerSchoolSearchTerm)
                );
            }

            Schools = await schoolQuery.ToListAsync();

            //Driversøgning
            var driverQuery = _context.Driver
                .Include(d => d.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(DriverSearchTerm))
            {
                var lowerDriverSearchTerm = DriverSearchTerm.ToLower();
                driverQuery = driverQuery.Where(d =>
                    d.DriverLicense.ToLower().Contains(lowerDriverSearchTerm) ||
                    d.DriverCpr.ToLower().Contains(lowerDriverSearchTerm) ||
                    d.User.FirstName.ToLower().Contains(lowerDriverSearchTerm) ||
                    d.User.LastName.ToLower().Contains(lowerDriverSearchTerm) ||
                    d.User.Email.ToLower().Contains(lowerDriverSearchTerm)
                );
            }

            Drivers = await driverQuery.ToListAsync();

            var tripQuery = _context.Trip
                .Include(t => t.Car.Driver) 
                .ThenInclude(d => d.User)   
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(TripSearchTerm))
            {
                var lowerTripSearchTerm = TripSearchTerm.ToLower();
                tripQuery = tripQuery.Where(t =>
                    t.FromDestination.ToLower().Contains(lowerTripSearchTerm) ||
                    t.ToDestination.ToLower().Contains(lowerTripSearchTerm) ||
                    t.TripDate.ToString().Contains(lowerTripSearchTerm) ||
                    t.TripTime.ToString().Contains(lowerTripSearchTerm) ||
                    t.TripSeats.ToString().Contains(lowerTripSearchTerm) ||
                    t.Car.Driver.DriverLicense.ToLower().Contains(lowerTripSearchTerm) ||
                    t.Car.Driver.User.FirstName.ToLower().Contains(lowerTripSearchTerm) ||
                    t.Car.Driver.User.LastName.ToLower().Contains(lowerTripSearchTerm)
                );
            }

            Trips = await tripQuery.ToListAsync();

            //Requestsøgning
            var requestQuery = _context.Request
                .Include(r => r.Users)
                .Include(r => r.Trip)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(RequestSearchTerm))
            {
                var lowerRequestSearchTerm = RequestSearchTerm.ToLower();

                requestQuery = requestQuery.Where(r =>

                    (r.RequestMessage != null && r.RequestMessage.ToLower().Contains(lowerRequestSearchTerm)) ||

                    (r.RequestAddress != null && r.RequestAddress.ToLower().Contains(lowerRequestSearchTerm)) ||

                    r.RequestTime.ToString().Contains(lowerRequestSearchTerm) ||

                    (
                        (lowerRequestSearchTerm == "accepteret") && r.RequestStatus == true ||
                        (lowerRequestSearchTerm == "afslået") && r.RequestStatus == false ||
                        (lowerRequestSearchTerm == "afventer" && r.RequestStatus == null)
                    ) ||

                    (r.Users != null && (
                        r.Users.FirstName.ToLower().Contains(lowerRequestSearchTerm) ||
                        r.Users.LastName.ToLower().Contains(lowerRequestSearchTerm) ||
                        r.Users.Email.ToLower().Contains(lowerRequestSearchTerm)
                    )) ||

                    (r.Trip != null && (
                        r.Trip.FromDestination.ToLower().Contains(lowerRequestSearchTerm) ||
                        r.Trip.ToDestination.ToLower().Contains(lowerRequestSearchTerm)
                    ))
                );
            }

            Requests = await requestQuery.ToListAsync();



            return Page();

        }

        private async Task<bool> IsAddressInUseAsync(int addressId)
        {
            bool usedInSchools = await _context.School.AnyAsync(s => s.AddressId == addressId);

            bool usedInUserAddresses = await _context.UserAddress.AnyAsync(ua => ua.Address_Id == addressId);

            return usedInSchools || usedInUserAddresses;
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
