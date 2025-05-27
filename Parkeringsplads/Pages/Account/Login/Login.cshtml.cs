using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Parkeringsplads.Models;
using Parkeringsplads.Services.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Parkeringsplads.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly IUser _userService;
        private readonly IDriverService _driverService;

        public LoginModel( IUser userService, IDriverService driverService)
        { 
            _userService = userService;
            _driverService = driverService;
        }

        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        public string Password { get; set; }

        public IActionResult OnGet() 
        {
            var UserEmail = HttpContext.Session.GetString("UserEmail");

            if (!string.IsNullOrEmpty(UserEmail))
            {
                return RedirectToPage("/Account/Profile");
            }

            return Page();
        }


       public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userService.GetUserByEmailAsync(Email);

            if (user != null && VerifyPassword(Password, user.Password))
            {
                HttpContext.Session.SetInt32("UserId", user.UserId);

                if (user.Title == "A" || user.Title == "a")
                {
                    HttpContext.Session.SetString("IsAdmin", "true");

                    HttpContext.Session.SetString("IsDriver", "true");
                }


                else
                {
                    var driver = await _driverService.GetDriverByUserIdAsync(user.UserId);

                    if (driver != null)
                    {
                        HttpContext.Session.SetString("IsDriver", driver.DriverId.ToString());
                    }

                }

                HttpContext.Session.SetString("UserEmail", user.Email);

                TempData["SuccessMessage"] = "Du er nu logget ind.";

                return RedirectToPage("/Account/Profile");
            }

            TempData["ErrorMessage"] = "Indtastet email eller adgangskode er forkert.";
            return Page();
        }


        private bool VerifyPassword(string enteredPassword, string storedPasswordHash)
        {
            return BCrypt.Net.BCrypt.Verify(enteredPassword, storedPasswordHash);
        }
    }
}
