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
        private readonly ParkeringspladsContext _context;

        public AddDriverAdminModel(IDriverService driverService, ParkeringspladsContext context)
        {
            _driverService = driverService;
            _context = context;
        }

        public List<User> UsersNotDrivers { get; set; } = new List<User>();
        public List<SelectListItem> Users { get; set; } = new List<SelectListItem>();  // <-- Define the Users property

        [BindProperty]
        public Driver NewDriver { get; set; }

        // Fetch the list of users who are NOT drivers
        public async Task<IActionResult> OnGetAsync()
        {
            var isAdmin = HttpContext.Session.GetString("IsAdmin");

            if (string.IsNullOrEmpty(isAdmin) || isAdmin != "true")
            {
                return RedirectToPage("/Admin/NotAdmin");
            }

            // Fetch users who are not already drivers
            UsersNotDrivers = await _context.User
                .Where(u => !_context.Driver.Any(d => d.UserId == u.UserId))
                .ToListAsync();

            // Convert users to SelectListItem for the dropdown
            Users = UsersNotDrivers.Select(u => new SelectListItem
            {
                Value = u.UserId.ToString(),
                Text = u.FirstName + " " + u.LastName // assuming you have FirstName and LastName in your User model
            }).ToList();

            return Page();
        }

        // Action to turn a user into a driver
        public async Task<IActionResult> OnPostAsync()
        {
            

            try
            {
                // Ensure the UserId is passed and that it's valid
                if (NewDriver.UserId == 0)
                {
                    TempData["ErrorMessage"] = "Vælg en bruger for chaufføren.";
                    return Page();
                }

                // Fetch the user using the passed UserId
                var user = await _context.User.FindAsync(NewDriver.UserId);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "Brugeren blev ikke fundet.";
                    return Page();
                }

                // Create a new driver and link it with the user
                var driver = new Driver
                {
                    UserId = NewDriver.UserId,
                    DriverLicense = NewDriver.DriverLicense,
                    DriverCpr = NewDriver.DriverCpr
                };

                // Save the new driver to the database
                _context.Driver.Add(driver);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Chauffør tilføjet med succes!";
                return RedirectToPage("/Admin/AdminDashboard");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Der opstod en fejl under tilføjelsen af chaufføren: " + ex.Message;
                return Page();
            }
        }
    }
}
