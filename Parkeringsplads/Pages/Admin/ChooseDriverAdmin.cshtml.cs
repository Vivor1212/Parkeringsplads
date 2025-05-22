
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Parkeringsplads.Models;


namespace Parkeringsplads.Pages.Admin
{
    public class ChooseDriverModel : PageModel
    {
        private readonly ParkeringspladsContext _context;

        public ChooseDriverModel(ParkeringspladsContext context)
        {
            _context = context;
        }

        public List<Driver> Drivers { get; set; } = new();
        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var isAdmin = HttpContext.Session.GetString("IsAdmin");

            if (string.IsNullOrEmpty(isAdmin) || isAdmin != "true")
            {
                return RedirectToPage("/Admin/NotAdmin");
            }

            Drivers = await _context.Driver
                .Include(d => d.User)
                .ToListAsync();

            var query = _context.Driver
        .Include(d => d.User)
        .AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                query = query.Where(d =>
    d.DriverId.ToString().Contains(SearchTerm) ||
    d.User.FirstName.ToLower().Contains(SearchTerm.ToLower()) ||
    d.User.LastName.ToLower().Contains(SearchTerm.ToLower()));
            }

            Drivers = await query.ToListAsync();

            return Page();
        }
    }
}
