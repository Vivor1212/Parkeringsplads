using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Parkeringsplads.Models;
using Parkeringsplads.Services.Interfaces;

namespace Parkeringsplads.Pages.CarPages
{
    public class CarEditModel : PageModel
    {
        private readonly ICarService _carService;

        public CarEditModel(ICarService carService)
        {
            _carService = carService;
        }


        public string ErrorMessage { get; set; }

        [BindProperty]
        public Car CarToEdit { get; set; }

        public async Task<IActionResult> OnGetAsync(int carId)
        {
            CarToEdit = await _carService.GetCarByIdAsync(carId);

            if (CarToEdit == null)
            {
                TempData["ErrorMessage"] = "Bil ikke fundet.";
                return RedirectToPage("/Account/Proile");
            }
            return Page();
        }
        public async Task<IActionResult> OnPostUpdateAsync()
        {
            Console.WriteLine($"Updating car: {CarToEdit.CarId}, {CarToEdit.CarModel}, {CarToEdit.CarPlate}, {CarToEdit.CarCapacity}");

            try
            {
                await _carService.UpdateCarAsync(CarToEdit);

                TempData["SuccessMessage"] = "Bil er opdateret.";

                var isAdmin = HttpContext.Session.GetString("IsAdmin");
                var userEmail = HttpContext.Session.GetString("UserEmail");

                if (isAdmin == "true")
                {
                    return RedirectToPage("/Admin/Admindashboard");
                }
                else if (!string.IsNullOrEmpty(userEmail))
                {
                    return RedirectToPage("/CarPages/Car");
                }
                return RedirectToPage("/Account/login/login");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Fejl ved opdatering af bil: " + ex.Message;
                return RedirectToPage();
            }
        }
    }
}
