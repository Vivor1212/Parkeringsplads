using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Parkeringsplads.Models;
using System.Linq;

namespace Parkeringsplads.Pages.Account
{
    public class ProfileModel : PageModel
    {
        private readonly ParkeringspladsContext _context;

        public ProfileModel(ParkeringspladsContext context)
        {
            _context = context;
        }

        public Driver? Driver { get; set; }
        public string UserEmail { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Title { get; set; }
        public bool IsDriver { get; set; }

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
            var user = _context.User
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

            //Check if user is a driver
            // Load the Driver object if it exists
            var driver = _context.Driver.FirstOrDefault(d => d.UserId == user.UserId);
            IsDriver = driver != null;
            Driver = driver;

            return Page(); // Return the Profile page with the user's information
        }
    }
}
