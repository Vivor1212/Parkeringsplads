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
            var user = await _context.User.FirstOrDefaultAsync(u => u.Email == Email);

            if (user != null && VerifyPassword(Password, user.Password))
            {

                if (user.Title == "A" || user.Title == "a")
                {
                    HttpContext.Session.SetString("IsAdmin", "true");

                    HttpContext.Session.SetString("IsDriver", "true");
                }


                else
                {
                    var driver = await _context.Driver.FirstOrDefaultAsync(d => d.UserId == user.UserId);

                    if (driver != null)
                    {
                        HttpContext.Session.SetString("IsDriver", driver.DriverId.ToString());
                    }

                }

                HttpContext.Session.SetString("UserEmail", user.Email);

                return RedirectToPage("/Account/Profile");
            }

            ErrorMessage = "Invalid email or password.";
            return Page();
        }


        private bool VerifyPassword(string enteredPassword, string storedPasswordHash)
        {
            return BCrypt.Net.BCrypt.Verify(enteredPassword, storedPasswordHash);
        }
    }
}
