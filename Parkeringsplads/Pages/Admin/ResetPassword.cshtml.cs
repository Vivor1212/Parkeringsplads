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
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(NewPassword))
            {
                ErrorMessage = "Both email and new password are required.";
                return Page();
            }

            var user = _context.User.FirstOrDefault(u => u.Email == Email);

            if (user == null)
            {
                ErrorMessage = "User not found.";
                return Page();
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(NewPassword);

            _context.SaveChanges();

            SuccessMessage = "Password updated successfully.";

            return Page();
        }
    }
}
