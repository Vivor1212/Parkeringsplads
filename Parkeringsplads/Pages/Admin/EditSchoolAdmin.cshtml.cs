using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Parkeringsplads.Models;
using Parkeringsplads.Services.Interfaces;

namespace Parkeringsplads.Pages.Admin
{
    public class EditSchoolAdminModel : PageModel
    {
        private readonly ISchoolService _schoolService;
        private readonly IAddressService _addressService;
        private readonly ICityService _cityService;

        public EditSchoolAdminModel(ISchoolService schoolService, IAddressService addressService, ICityService cityService)
        {
            _schoolService = schoolService;
            _addressService = addressService;
            _cityService = cityService;
        }

        [BindProperty]
        public int SchoolId { get; set; }

        [BindProperty]
        public string SchoolName { get; set; } = string.Empty;

        [BindProperty]
        public string AddressRoad { get; set; } = string.Empty;

        [BindProperty]
        public string AddressNumber { get; set; } = string.Empty;

        [BindProperty]
        public int SelectedCityId { get; set; }

        public List<SelectListItem> Cities { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? schoolId)
        {
            var isAdmin = HttpContext.Session.GetString("IsAdmin");

            if (string.IsNullOrEmpty(isAdmin) || isAdmin != "true")
            {
                return RedirectToPage("/Admin/NotAdmin");
            }

            if (schoolId == null || schoolId == 0)
            {
                TempData["ErrorMessage"] = "Skole-ID mangler.";
                return RedirectToPage("/Admin/AdminDashboard");
            }

            SchoolId = schoolId.Value;
            var school = await _schoolService.GetSchoolWithAddressAsync(SchoolId);
            if (school == null)
            {
                TempData["ErrorMessage"] = "Skolen blev ikke fundet.";
                return RedirectToPage("/Admin/AdminDashboard");
            }

            SchoolName = school.SchoolName;
            AddressRoad = school.Address?.AddressRoad ?? "";
            AddressNumber = school.Address?.AddressNumber ?? "";
            SelectedCityId = school.Address?.CityId ?? 0;

            await LoadCitiesAsync();
            return Page();
        }


        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                await LoadCitiesAsync();

                if (string.IsNullOrWhiteSpace(SchoolName) || string.IsNullOrWhiteSpace(AddressRoad) || string.IsNullOrWhiteSpace(AddressNumber) || SelectedCityId == 0)
                {
                    TempData["ErrorMessage"] = "Udfyld alle felter korrekt.";
                    return Page();
                }

                var newAddress = new Address
                {
                    AddressRoad = AddressRoad,
                    AddressNumber = AddressNumber,
                    CityId = SelectedCityId
                };

                var createdAddress = await _addressService.CreateAddressAsync(newAddress);
                var success = await _schoolService.UpdateSchoolAsync(SchoolId, SchoolName, createdAddress.AddressId);
                if (!success)
                {
                    TempData["ErrorMessage"] = "Skolen blev ikke fundet.";
                    return Page();
                }

                TempData["SuccessMessage"] = "Skolen er opdateret.";
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
