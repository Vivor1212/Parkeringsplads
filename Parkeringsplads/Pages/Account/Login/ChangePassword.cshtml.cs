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

        [BindProperty]
        public string CurrentPassword { get; set; }

        [BindProperty]
        public string NewPassword { get; set; }

        [BindProperty]
        public string ConfirmPassword { get; set; }


        public IActionResult OnGet()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToPage("./Login");
            }

            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToPage("./Login/Login");
            }

            var user = _context.User.FirstOrDefault(u => u.Email == userEmail);

            if (user == null)
            {
                return RedirectToPage("./Login/Login");
            }

            if (!BCrypt.Net.BCrypt.Verify(CurrentPassword, user.Password))
            {
                TempData["ErrorMessage"] = "Current password is incorrect.";
                return Page();
            }

            if (NewPassword != ConfirmPassword)
            {
                TempData["ErrorMessage"] = "New password and confirmation password do not match.";
                return Page();
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(NewPassword);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Password changed successfully.";

            return RedirectToPage("/Account/Profile");
        }
    }
}
