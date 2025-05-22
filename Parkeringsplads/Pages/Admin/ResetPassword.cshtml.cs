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

        [BindProperty]
        public string ConfirmNewPassword { get; set; }


        public async Task<IActionResult> OnGetAsync()
        {

            var isAdmin = HttpContext.Session.GetString("IsAdmin");

            if (string.IsNullOrEmpty(isAdmin) || isAdmin != "true")
            {
                return RedirectToPage("/Admin/NotAdmin");
            }
            return Page();
        }

        public IActionResult OnPost()
        {
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(NewPassword) || string.IsNullOrEmpty(ConfirmNewPassword))
            {
                TempData["ErrorMessage"] = "Both email and passwords are required.";
                return Page();
            }

            if (NewPassword != ConfirmNewPassword)
            {
                TempData["ErrorMessage"] = "Passwords do not match.";
                return Page();
            }

            var user = _context.User.FirstOrDefault(u => u.Email == Email);

            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return Page();
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(NewPassword);

            _context.SaveChanges();

            TempData["SuccessMessage"] = "Password updated successfully.";

            return RedirectToPage("/Admin/AdminDashboard");
        }
    }
}
