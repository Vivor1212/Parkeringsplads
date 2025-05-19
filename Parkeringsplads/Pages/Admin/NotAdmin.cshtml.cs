using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Parkeringsplads.Pages.Admin
{
    public class NotAdminModel : PageModel
    {
        public IActionResult OnGet()
        {

            HttpContext.Session.Clear();
            return Page();
        }
    }
}
