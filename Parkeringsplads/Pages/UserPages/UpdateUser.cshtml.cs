using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Parkeringsplads.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Parkeringsplads.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Parkeringsplads.Pages.UserPages
{
    public class UpdateUserModel : PageModel
    {
        private readonly ParkeringspladsContext _context;
        private readonly IUser _userService;
        private readonly ISchoolService _schoolService;

        public UpdateUserModel(IUser userService, ParkeringspladsContext context, ISchoolService schoolService)
        {
            _context = context;
            _userService = userService;
            _schoolService = schoolService;


        }



        [BindProperty]
        public User User { get; set; }
        public string UserEmail { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Title { get; set; }
        public School School { get; set; }

        public string SchoolName { get; set; }
        public List<SelectListItem> Schools { get; set; }


        private async Task LoadDropdownDataAsync()
        {
            // Fetch the dropdown data
            Schools = await _schoolService.SchoolDropDownAsync();
            // You can similarly fetch City dropdown here if required (using _schoolService or another service).
        }

        public async Task<IActionResult> OnGetAsync()
        {

            await LoadDropdownDataAsync();

            // Retrieve user email from session
            var userEmail = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrEmpty(userEmail))
            {
                // If no user is logged in, redirect to login page
                return RedirectToPage("/Account/Login/Login");
            }
            var userSchool = _context.User
                .Include(u => u.School) // Include the School navigation property
                .FirstOrDefault(u => u.Email == userEmail);
            // Query the database to retrieve user information based on the email
            var user = _context.User
                               .Where(u => u.Email == userEmail)
                               .FirstOrDefault();

            if (user == null)
            {
                // If no user found in the database, redirect to login page
                return RedirectToPage("/Account/Login/Login");
            }

           
            // Assign the retrieved user data to the properties
            UserEmail = user.Email;
            FirstName = user.FirstName;
            LastName = user.LastName;
            Phone = user.Phone;
            Title = user.Title;
            School = user.School; // Assign the School entity

            SchoolName = user.School?.SchoolName; // Use the null conditional operator to avoid null reference errors



            return Page(); // Return the Profile page with the user's information
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");

            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToPage("/Account/Login/Login");
            }

            var userInDb = _context.User.FirstOrDefault(u => u.Email == userEmail);

          
            if (userInDb == null)
            {
                return RedirectToPage("/Account/Login/Login");
            }

            User.UserId = userInDb.UserId;

            // Attempt to update via the service
            bool updateSuccessful = await _userService.UpdateUserAsync(User);

            if (!updateSuccessful)
            {
                ModelState.AddModelError(string.Empty, "The email is already in use by another user.");
                return Page(); // Return the page with error message
            }

            return RedirectToPage("/Account/Profile");
        }

    }
}
