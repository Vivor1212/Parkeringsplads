
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Parkeringsplads.Models;
using Parkeringsplads.Services.Interfaces;


namespace Parkeringsplads.Pages.Admin
{
    public class ChooseDriverModel : PageModel
    {
        private readonly IDriverService _driverService;

        public ChooseDriverModel(IDriverService driverService)
        {
            _driverService = driverService;
        }

        public List<Driver> Drivers { get; set; } = new();
        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var isAdmin = HttpContext.Session.GetString("IsAdmin") == "true";
            if (!isAdmin)
            {
                return RedirectToPage("/Admin/NotAdmin");
            }

            Drivers = await _driverService.GetDriversWithUserAsync(SearchTerm);

            return Page();
        }
    }
}
