// Pages/Admin/SelectDriver.cshtml.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Parkeringsplads.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        public async Task<IActionResult> OnGetAsync()
        {
            Drivers = await _context.Driver
                .Include(d => d.User)
                .ToListAsync();

            return Page();
        }
    }
}
