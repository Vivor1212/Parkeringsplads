using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Parkeringsplads.Models;
using Parkeringsplads.Services.Interfaces;
using System.Linq;

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

        [BindProperty]
        public string CarModelName { get; set; }

        [BindProperty]
        public string CarPlate { get; set; }

        [BindProperty]
        public int CarCapacity { get; set; }

        [BindProperty]
        public int? SelectedDriverId { get; set; }  

        public List<SelectListItem> Drivers { get; set; }

        public async Task OnGetAsync()
        {
            string isAdminString = HttpContext.Session.GetString("IsAdmin");

            if (isAdminString == "true")
            {
                Drivers = await _driverService.GetAllDriversAsync();  
            }
            else
            {
                string driverIdString = HttpContext.Session.GetString("IsDriver");

                if (string.IsNullOrEmpty(driverIdString))
                {
                    TempData["ErrorMessage"] = "Ikke registreret som chauffør.";
                    Cars = new List<Car>();
                    RedirectToPage("/Account/Profile");
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
                    TempData["ErrorMessage"] = "Vælg chauffør";
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
                    TempData["ErrorMessage"] = "Ikke registreret som chauffør";
                    RedirectToPage("/Account/Profile");
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

                TempData["SuccessMessage"] = "Bil tilføjet.";

                if (isAdmin == "true")
                {
                    return RedirectToPage("/Admin/AdminDashboard");
                }

                Cars = await _carService.GetCarsByDriverIdAsync(driverId);
                return RedirectToPage("/CarPages/Car");
            }
            catch (Exception ex) 
            {
                TempData["ErrorMessage"] = "Der opstod en fejl" + ex.Message;
                if (isAdmin == "true")
                    Drivers = await _driverService.GetAllDriversAsync();
                return Page();
            }
        }


        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            
            try
            {
                await _carService.DeleteCarAsync(id);

                TempData["SuccessMessage"] = "Bil slettet.";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Fejl ved sletning af bil: " + ex.Message;
                return RedirectToPage();
            }
        }

    }
}
