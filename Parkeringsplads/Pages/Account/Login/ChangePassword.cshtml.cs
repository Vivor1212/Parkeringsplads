using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Parkeringsplads.Models;
using BCrypt.Net;

namespace Parkeringsplads.Pages.Account
{
    public class ChangePasswordModel : PageModel
    {
        private readonly ParkeringspladsContext _context;

        public ChangePasswordModel(ParkeringspladsContext context)
        {
            _context = context;
        }

        // Properties for current password, new password, and confirm password
        [BindProperty]
        public string CurrentPassword { get; set; }

        [BindProperty]
        public string NewPassword { get; set; }

        [BindProperty]
        public string ConfirmPassword { get; set; }

        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }

        public IActionResult OnGet()
        {
            // Check if the user is logged in by checking session
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToPage("/Login");
            }

            return Page();
        }

        public IActionResult OnPost()
        {
            // Retrieve user email from session
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToPage("/Login");
            }

            // Retrieve user from the database based on email
            var user = _context.User.FirstOrDefault(u => u.Email == userEmail);

            if (user == null)
            {
                return RedirectToPage("/Login");
            }

            // Verify the current password
            if (!BCrypt.Net.BCrypt.Verify(CurrentPassword, user.Password))
            {
                ErrorMessage = "Current password is incorrect.";
                return Page();
            }

            // Check if the new password and confirm password match
            if (NewPassword != ConfirmPassword)
            {
                ErrorMessage = "New password and confirmation password do not match.";
                return Page();
            }

            // Hash the new password and update the user record
            user.Password = BCrypt.Net.BCrypt.HashPassword(NewPassword);

            // Save changes to the database
            _context.SaveChanges();

            // Set a success message
            SuccessMessage = "Password changed successfully.";

            // Redirect to the profile page (or wherever you'd like)
            return RedirectToPage("/Profile");
        }
    }
}
