using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Parkeringsplads.Models;
using Parkeringsplads.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Parkeringsplads.Pages.Admin
{
    public class AddDriverAdminModel : PageModel
    {
        private readonly IDriverService _driverService;
        private readonly IUser _userService;

        public AddDriverAdminModel(IDriverService driverService, IUser userService)
        {
            _driverService = driverService;
            _userService = userService;
        }

        public List<User> UsersNotDrivers { get; set; } = new List<User>();
        public List<SelectListItem> Users { get; set; } = new List<SelectListItem>();

        [BindProperty]
        public Driver NewDriver { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var isAdmin = HttpContext.Session.GetString("IsAdmin") == "true";
            if (!isAdmin)
            {
                return RedirectToPage("/Admin/NotAdmin");
            }

            Users = await _userService.GetNonDriverUsersAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var isAdmin = HttpContext.Session.GetString("IsAdmin") == "true";
            if (!isAdmin)
            {
                return RedirectToPage("/Admin/NotAdmin");
            }

            try
            {
                var driverResult = await _driverService.ValidateAndCreateDriverAdync(NewDriver);
                if (!driverResult.IsValid)
                {
                    Users = await _userService.GetNonDriverUsersAsync();
                    TempData["ErrorMessage"] = driverResult.ErrorMessage;
                    return Page();
                }

                TempData["SuccessMessage"] = "Chauffør tilføjet med succes!";
                return RedirectToPage("/Admin/AdminDashboard");
            }
            catch (Exception ex)
            {
                Users = await _userService.GetNonDriverUsersAsync();
                TempData["ErrorMessage"] = $"Fejl ved tilføjelse af chauffør: {ex.Message}";
                return Page();
            }
        }

    }
}
