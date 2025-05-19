using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Parkeringsplads.Models;

namespace Parkeringsplads.Pages.Admin
{
    public class DriverAdminModel : PageModel
    {
        public async Task<IActionResult> OnGetAsync()
        {
            var isAdmin = HttpContext.Session.GetString("IsAdmin");

            if (string.IsNullOrEmpty(isAdmin) || isAdmin != "true")
            {
                // User is not an admin, redirect to login
                return RedirectToPage("/Admin/NotAdmin");
            }
            return Page();
        }
    }
}
