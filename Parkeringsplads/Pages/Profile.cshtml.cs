using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Parkeringsplads.Models;
using System.Linq;

namespace Parkeringsplads.Pages
{
    public class ProfileModel : PageModel
    {
        private readonly TestParkeringspladsContext _context;

        public ProfileModel(TestParkeringspladsContext context)
        {
            _context = context;
        }

        public string UserEmail { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Title { get; set; }

        public IActionResult OnGet()
        {
            // Retrieve user email from session
            var userEmail = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrEmpty(userEmail))
            {
                // If no user is logged in, redirect to login page
                return RedirectToPage("/Login");
            }

            // Query the database to retrieve user information based on the email
            var user = _context.Users
                               .Where(u => u.Email == userEmail)
                               .FirstOrDefault();

            if (user == null)
            {
                // If no user found in the database, redirect to login page
                return RedirectToPage("/Login");
            }

            // Assign the retrieved user data to the properties
            UserEmail = user.Email;
            FirstName = user.FirstName;
            LastName = user.LastName;
            Phone = user.Phone;
            Title = user.Title;

            return Page(); // Return the Profile page with the user's information
        }
    }
}
