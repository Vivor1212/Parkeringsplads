using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Parkeringsplads.Models;
using Parkeringsplads.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Parkeringsplads.Pages.Admin
{
    public class AddSchoolAdminModel : PageModel
    {
        private readonly ISchoolService _schoolService;
        private readonly ICityService _cityService;

        public AddSchoolAdminModel(ISchoolService schoolService, ICityService cityService)
        {
            _schoolService = schoolService;
            _cityService = cityService;
        }

        [BindProperty]
        public string SchoolName { get; set; } = string.Empty;

        [BindProperty]
        public string AddressRoad { get; set; } = string.Empty;

        [BindProperty]
        public string AddressNumber { get; set; } = string.Empty;

        [BindProperty]
        public int SelectedCityId { get; set; }

        public List<SelectListItem> Cities { get; set; } = new();

        
        public async Task<IActionResult> OnGetAsync()
        {
            await LoadCitiesAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var isAdmin = HttpContext.Session.GetString("IsAdmin");

            if (string.IsNullOrEmpty(isAdmin) || isAdmin != "true")
            {
                return RedirectToPage("/Admin/NotAdmin");
            }

            await LoadCitiesAsync();

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Udfyld alle felter korrekt.";
                return Page();
            }

            try
            {
                await _schoolService.CreateSchoolAsync(SchoolName, AddressRoad, AddressNumber, SelectedCityId);
                TempData["SuccessMessage"] = "Skolen er oprettet.";
                return RedirectToPage("/Admin/AdminDashboard"); 
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Der opstod en fejl: {ex.Message}";
                return Page();
            }
        }

        private async Task LoadCitiesAsync()
        {
            Cities = await _cityService.CityDropDownAsync();
        }
    }
}
