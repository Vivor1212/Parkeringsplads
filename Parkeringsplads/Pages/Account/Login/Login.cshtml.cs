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
            // Find user by email
            var user = await _context.User.FirstOrDefaultAsync(u => u.Email == Email);

            if (user != null && VerifyPassword(Password, user.Password))
            {
                // Password matches, set session and redirect
                HttpContext.Session.SetString("UserEmail", user.Email);
                return RedirectToPage("/Account/Profile"); // Redirect to a protected page
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
