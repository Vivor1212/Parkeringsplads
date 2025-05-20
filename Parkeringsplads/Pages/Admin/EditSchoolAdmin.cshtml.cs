using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Parkeringsplads.Models;

namespace Parkeringsplads.Pages.Admin
{
    public class EditSchoolAdminModel : PageModel
    {
        private readonly ParkeringspladsContext _context;

        public EditSchoolAdminModel(ParkeringspladsContext context)
        {
            _context = context;
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

        [TempData]
        public string? SuccessMessage { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int? schoolId)
        {
            var isAdmin = HttpContext.Session.GetString("IsAdmin");

            if (string.IsNullOrEmpty(isAdmin) || isAdmin != "true")
            {
                return RedirectToPage("/Admin/NotAdmin");
            }

            ErrorMessage = null;

            if (schoolId == null || schoolId == 0)
            {
                ErrorMessage = "Skole-ID mangler.";
                return RedirectToPage("/Admin/AdminDashboard");
            }

            SchoolId = schoolId.Value;

            var school = await _context.School
                .Include(s => s.Address)
                    .ThenInclude(a => a.City)
                .FirstOrDefaultAsync(s => s.SchoolId == SchoolId);

            if (school == null)
            {
                ErrorMessage = "Skolen blev ikke fundet.";
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
            await LoadCitiesAsync();

            if (!ModelState.IsValid)
            {
                ErrorMessage = "Udfyld alle felter korrekt.";
                return Page();
            }

            var school = await _context.School.FirstOrDefaultAsync(s => s.SchoolId == SchoolId);
            if (school == null)
            {
                ErrorMessage = "Skolen blev ikke fundet.";
                return Page();
            }

            var newAddress = new Address
            {
                AddressRoad = AddressRoad,
                AddressNumber = AddressNumber,
                CityId = SelectedCityId
            };

            _context.Address.Add(newAddress);
            await _context.SaveChangesAsync();

            school.SchoolName = SchoolName;
            school.AddressId = newAddress.AddressId;

            await _context.SaveChangesAsync();

            SuccessMessage = "Skolen er opdateret.";
            return RedirectToPage();
        }

        private async Task LoadCitiesAsync()
        {
            Cities = await _context.City
                .OrderBy(c => c.PostalCode)
                .Select(c => new SelectListItem
                {
                    Value = c.CityId.ToString(),
                    Text = $"{c.PostalCode} {c.CityName}"
                }).ToListAsync();
        }
    }
}
