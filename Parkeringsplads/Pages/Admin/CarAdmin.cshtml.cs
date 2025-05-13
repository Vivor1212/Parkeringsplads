using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Parkeringsplads.Models;
using Parkeringsplads.Services.Interfaces;

namespace Parkeringsplads.Pages.Admin
{
    public class CarAdminModel : PageModel
    {

        private readonly ICarService _carService;

        public CarAdminModel(ICarService carService)
        {
            _carService = carService;
        }

        public IList<Car> Car { get; set; }

        public string ErrorMessage { get; set; }

        [BindProperty]
        public string CarModelName { get; set; }

        [BindProperty]
        public string CarPlate { get; set; }

        [BindProperty]
        public int CarCapacity { get; set; }

        public async Task OnGetAsync()
        {
            Car = await _carService.GetAllCarsAsync();
        }

        
        public async Task<IActionResult> OnPostDeleteAsync(int carId)
        {
            try
            {
                await _carService.DeleteCarAsync(carId);

                return RedirectToPage();
            }
            catch
            {
                return Page();
            }
        }
    }
}
