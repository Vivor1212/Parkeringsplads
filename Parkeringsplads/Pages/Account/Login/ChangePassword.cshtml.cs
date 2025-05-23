using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Parkeringsplads.Models;
using BCrypt.Net;
using Parkeringsplads.Services.Interfaces;

namespace Parkeringsplads.Pages.Account
{
    public class ChangePasswordModel : PageModel
    {
        private readonly IUser _userService;

        public ChangePasswordModel(IUser userService)
        {
            _userService = userService;
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

            if (NewPassword != ConfirmPassword)
            {
                TempData["ErrorMessage"] = "New password and confirmation password do not match.";
                return Page();
            }

            var success = await _userService.ChangePasswordAsync(userEmail, CurrentPassword, NewPassword);
            if (!success)
            {
                TempData["ErrorMessage"] = "Current password is incorrect or user not found.";
                return Page();
            }

            TempData["SuccessMessage"] = "Password changed successfully.";
            return RedirectToPage("/Account/Profile");
        }
    }
}
