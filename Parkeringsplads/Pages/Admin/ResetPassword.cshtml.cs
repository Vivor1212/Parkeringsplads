using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Parkeringsplads.Models;
using BCrypt.Net;

namespace Parkeringsplads.Pages.Admin
{
    public class ResetPasswordModel : PageModel
    {
        private readonly ParkeringspladsContext _context;

        public ResetPasswordModel(ParkeringspladsContext context)
        {
            _context = context;
        }

        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        public string NewPassword { get; set; }

        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }

        public IActionResult OnGet()
        {
            // If needed, you can put additional logic here
            return Page();
        }

        public IActionResult OnPost()
        {
            // Check if both fields are filled out
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(NewPassword))
            {
                ErrorMessage = "Both email and new password are required.";
                return Page();
            }

            // Retrieve the user from the database using the email
            var user = _context.User.FirstOrDefault(u => u.Email == Email);

            if (user == null)
            {
                ErrorMessage = "User not found.";
                return Page();
            }

            // Hash the new password
            user.Password = BCrypt.Net.BCrypt.HashPassword(NewPassword);

            // Save the changes to the database
            _context.SaveChanges();

            // Success message
            SuccessMessage = "Password updated successfully.";

            return Page();
        }
    }
}
