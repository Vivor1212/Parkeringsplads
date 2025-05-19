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
        private readonly ParkeringspladsContext _context;
        private readonly ISchoolService _schoolService;

        public AddSchoolAdminModel(ParkeringspladsContext context, ISchoolService schoolService)
        {
            _context = context;
            _schoolService = schoolService;
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

        [TempData]
        public string? SuccessMessage { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            await LoadCitiesAsync();
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
                ErrorMessage = "Udfyld alle felter korrekt.";
                return Page();
            }

            try
            {
                await _schoolService.CreateSchoolAsync(SchoolName, AddressRoad, AddressNumber, SelectedCityId);
                SuccessMessage = "Skolen er oprettet.";
                return RedirectToPage("/Admin/AdminDashboard"); 
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Der opstod en fejl: {ex.Message}";
                return Page();
            }
        }

        private async Task LoadCitiesAsync()
        {
            Cities = await _context.City
                .OrderBy(c => c.CityName) 
                .Select(c => new SelectListItem
                {
                    Value = c.CityId.ToString(),
                    Text = $"{c.CityName} - {c.PostalCode}" 
                }).ToListAsync();
        }
    }
}
