using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Parkeringsplads.Models;
using Parkeringsplads.Services.Interfaces;

namespace Parkeringsplads.Pages.Admin
{
    public class EditDriverAdminModel : PageModel
    {
        private readonly IDriverService _driverService;
        private readonly IUser _userService;

        public EditDriverAdminModel(IDriverService driverService, IUser userService)
        {
            _driverService = driverService;
            _userService = userService;
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

        public async Task<IActionResult> OnGetAsync(int? driverId)
        {
            var isAdmin = HttpContext.Session.GetString("IsAdmin");

            if (string.IsNullOrEmpty(isAdmin) || isAdmin != "true")
            {
                return RedirectToPage("/Admin/NotAdmin");
            }

            if (driverId == null || driverId == 0)
            {
                TempData["ErrorMessage"] = "Chauffør-ID mangler.";
                return RedirectToPage("/Admin/AdminDashboard");
            }

            DriverId = driverId.Value;
            var driver = await _driverService.GetDriverWithUserAsync(DriverId);
            if (driver == null)
            {
                TempData["ErrorMessage"] = "Chaufføren blev ikke fundet.";
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
            Users = await _userService.UserDropDownAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                var success = await _driverService.UpdateDriverAsync(DriverId, DriverLicense, DriverCpr, SelectedUserId);
                if (!success)
                {
                    await LoadUsersAsync();
                    TempData["ErrorMessage"] = "Chaufføren blev ikke fundet.";
                    return Page();
                }

                TempData["SuccessMessage"] = "Chaufføren er blevet opdateret.";
                return RedirectToPage("/Admin/AdminDashboard"); 
            }
            catch (Exception ex)
            {
                await LoadUsersAsync();
                TempData["ErrorMessage"] = "Der opstod en fejl ved opdatering af chaufføren.: " + ex.Message;
                return Page();
            }
        }
    }
}
