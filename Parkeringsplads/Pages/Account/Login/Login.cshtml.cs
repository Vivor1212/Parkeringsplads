using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Parkeringsplads.Models;
using System.Text;

namespace Parkeringsplads.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly ParkeringspladsContext _context;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(ParkeringspladsContext context, ILogger<LoginModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        public string Password { get; set; }

        public string ErrorMessage { get; set; }

        // Handle GET requests (Display the login form)
        public void OnGet() { }


       public async Task<IActionResult> OnPostAsync()
        {
            // Find the user by email
            var user = await _context.User.FirstOrDefaultAsync(u => u.Email == Email);

            if (user != null && VerifyPassword(Password, user.Password))
            {
                // Check if the user exists in the Driver table by referencing UserId
                var driver = await _context.Driver.FirstOrDefaultAsync(d => d.UserId == user.UserId);

                if (driver != null)
                {
                    // User is a driver, store the DriverId in the session
                    HttpContext.Session.SetString("IsDriver", driver.DriverId.ToString());
                }

                
                // If user is not a driver, do not set the session for IsDriver

                // Store the user's email in the session
                HttpContext.Session.SetString("UserEmail", user.Email);

                // Redirect to the Profile page
                return RedirectToPage("/Account/Profile");
            }

            // If login failed, show an error message
            ErrorMessage = "Invalid email or password.";
            return Page();
        }


        // Verify if the entered password matches the stored password hash
        private bool VerifyPassword(string enteredPassword, string storedPasswordHash)
        {
            // If you're using bcrypt for password hashing:
            return BCrypt.Net.BCrypt.Verify(enteredPassword, storedPasswordHash);
        }
    }
}
