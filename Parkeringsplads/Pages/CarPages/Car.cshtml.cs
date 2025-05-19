using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Parkeringsplads.Models;
using Parkeringsplads.Services.Interfaces;

namespace Parkeringsplads.Pages.CarPages
{
    public class CarModel : PageModel
    {
        private readonly ICarService _carService;
        private readonly IDriverService _driverService;

        public CarModel(ICarService carService, IDriverService driverService)
        {
            _carService = carService;
            _driverService = driverService;
        }

        public IList<Car> Cars { get; set; }

        public string ErrorMessage { get; set; }

        [BindProperty]
        public string CarModelName { get; set; }

        [BindProperty]
        public string CarPlate { get; set; }

        [BindProperty]
        public int CarCapacity { get; set; }

        [BindProperty]
        public int? SelectedDriverId { get; set; }  // New field for selecting a driver

        public List<SelectListItem> Drivers { get; set; }

        public async Task OnGetAsync()
        {
            string isAdminString = HttpContext.Session.GetString("IsAdmin");

            if (isAdminString == "true")
            {
                // If user is admin, load all drivers (users) to assign a car
                Drivers = await _driverService.GetAllDriversAsync();  // Assuming you have a method for getting all users
            }
            else
            {
                // If not admin, check if the user is a driver and load their cars
                string driverIdString = HttpContext.Session.GetString("IsDriver");

                if (string.IsNullOrEmpty(driverIdString))
                {
                    ErrorMessage = "You are not logged in as a driver.";
                    Cars = new List<Car>();
                    return;
                }

                int driverId = int.Parse(driverIdString);
                Cars = await _carService.GetCarsByDriverIdAsync(driverId);
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var isAdmin = HttpContext.Session.GetString("IsAdmin");

            int driverId;

            if (isAdmin == "true")
            {
                if (SelectedDriverId == null || SelectedDriverId == 0)
                {
                    ErrorMessage = "Please select a driver.";
                    Drivers = await _driverService.GetAllDriversAsync();
                    return Page();
                }

                driverId = SelectedDriverId.Value;
            }
            else
            {
                string driverIdString = HttpContext.Session.GetString("IsDriver");

                if (string.IsNullOrEmpty(driverIdString))
                {
                    ErrorMessage = "You are not logged in as a driver.";
                    return Page();
                }

                driverId = int.Parse(driverIdString);
            }

            var newCar = new Car
            {
                CarModel = CarModelName,
                CarPlate = CarPlate,
                CarCapacity = CarCapacity,
                DriverId = driverId
            };

            try
            {
                await _carService.AddCarToDriverAsync(driverId, newCar);

                if (isAdmin == "true")
                {
                    return RedirectToPage("/Admin/AdminDashboard");
                }

                Cars = await _carService.GetCarsByDriverIdAsync(driverId);
                return RedirectToPage("/CarPages/Car");
            }
            catch
            {
                ErrorMessage = "An error occurred while adding the car.";
                if (isAdmin == "true")
                    Drivers = await _driverService.GetAllDriversAsync();
                return Page();
            }
        }


        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            await _carService.DeleteCarAsync(id);
            return RedirectToPage();
        }

    }
}
