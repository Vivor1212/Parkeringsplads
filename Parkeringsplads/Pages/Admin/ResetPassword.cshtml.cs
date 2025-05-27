using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Parkeringsplads.Models;
using BCrypt.Net;
using Parkeringsplads.Services.Interfaces;
using System.Threading.Tasks;

namespace Parkeringsplads.Pages.Admin
{
    public class ResetPasswordModel : PageModel
    {
        private readonly IUser _userService;

        public ResetPasswordModel(IUser userService)
        {
            _userService = userService;
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

        public async Task<IActionResult> OnPost()
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

            var success = await _userService.ResetPasswordAsync(Email, NewPassword);
            if (!success)
            {
                TempData["ErrorMessage"] = "User not found.";
                return Page();
            }

            TempData["SuccessMessage"] = "Password reset successfully.";
            return RedirectToPage("/Admin/AdminDashboard");
        }
    }
}
