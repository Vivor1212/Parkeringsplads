using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Parkeringsplads.Models;

namespace Parkeringsplads.Pages.Admin
{
    public class EditDriverAdminModel : PageModel
    {
        private readonly ParkeringspladsContext _context;

        public EditDriverAdminModel(ParkeringspladsContext context)
        {
            _context = context;
        }

        [BindProperty]
        public int DriverId { get; set; }

        [BindProperty]
        public string DriverLicense { get; set; } = string.Empty;

        [BindProperty]
        public string DriverCpr { get; set; } = string.Empty;

        [BindProperty]
        public int SelectedUserId { get; set; }

        public List<SelectListItem> Users { get; set; } = new();

        public string SelectedDriverName { get; set; } = string.Empty;

        [TempData]
        public string? SuccessMessage { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int? driverId)
        {
            var isAdmin = HttpContext.Session.GetString("IsAdmin");

            if (string.IsNullOrEmpty(isAdmin) || isAdmin != "true")
            {
                return RedirectToPage("/Admin/NotAdmin");
            }

            ErrorMessage = null;

            if (driverId == null || driverId == 0)
            {
                ErrorMessage = "Chauffør-ID mangler.";
                return RedirectToPage("/Admin/AdminDashboard");
            }

            DriverId = driverId.Value;

            var driver = await _context.Driver
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.DriverId == DriverId);

            if (driver == null)
            {
                ErrorMessage = "Chaufføren blev ikke fundet.";
                return RedirectToPage("/Admin/AdminDashboard");
            }

            DriverLicense = driver.DriverLicense;
            DriverCpr = driver.DriverCpr;
            SelectedUserId = driver.UserId;
            SelectedDriverName = $"{driver.User.FirstName} {driver.User.LastName}";

            await LoadUsersAsync();
            return Page();
        }

        private async Task LoadUsersAsync()
        {
            Users = await _context.User
                .OrderBy(u => u.UserId)
                .Select(u => new SelectListItem
                {
                    Value = u.UserId.ToString(),
                    Text = $"{u.FirstName} {u.LastName} ({u.Email})"
                }).ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var driver = await _context.Driver.FirstOrDefaultAsync(d => d.DriverId == DriverId);

            if (driver == null)
            {
                ErrorMessage = "Chaufføren blev ikke fundet.";
                return Page();
            }

            driver.DriverLicense = DriverLicense;
            driver.DriverCpr = DriverCpr;
            driver.UserId = SelectedUserId;

            try
            {
                await _context.SaveChangesAsync();
                SuccessMessage = "Chaufføren er blevet opdateret.";
                return RedirectToPage("/Admin/AdminDashboard"); 
            }
            catch (Exception)
            {
                ErrorMessage = "Der opstod en fejl ved opdatering af chaufføren.";
                return Page();
            }
        }
    }
}
