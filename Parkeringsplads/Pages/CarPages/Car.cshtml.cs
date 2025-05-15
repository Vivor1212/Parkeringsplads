using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Parkeringsplads.Models;
using Parkeringsplads.Services.Interfaces;

namespace Parkeringsplads.Pages.CarPages
{
    public class CarModel : PageModel
    {
        private readonly ICarService _carService;

        public CarModel(ICarService carService)
        {
            _carService = carService;
        }

        public IList<Car> Cars { get; set; }

        public string ErrorMessage { get; set; }

        [BindProperty]
        public string CarModelName { get; set; }

        [BindProperty]
        public string CarPlate { get; set; }

        [BindProperty]
        public int CarCapacity { get; set; }

        public async Task OnGetAsync()
        {
            
            string driverIdString = HttpContext.Session.GetString("IsDriver");

            if (string.IsNullOrEmpty(driverIdString))
            {
                ErrorMessage = "You are not logged in as a driver.";
                Cars = new List<Car>();
                return;
            }

            int driverId = int.Parse(driverIdString);

            try
            {
                Cars = await _carService.GetCarsByDriverIdAsync(driverId);
            }
            catch
            {
                ErrorMessage = "An error occurred while loading your cars.";
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            
            string driverIdString = HttpContext.Session.GetString("IsDriver");

            if (string.IsNullOrEmpty(driverIdString))
            {
                ErrorMessage = "You are not logged in as a driver.";
                return Page();
            }

            int driverId = int.Parse(driverIdString);


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

                Cars = await _carService.GetCarsByDriverIdAsync(driverId);

                return RedirectToPage("/CarPages/Car");
            }
            catch
            {
                ErrorMessage = "An error occurred while adding the car.";
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
