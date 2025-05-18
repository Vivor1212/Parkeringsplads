using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Parkeringsplads.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Parkeringsplads.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Client;

namespace Parkeringsplads.Pages.UserPages
{
    public class UpdateUserModel : PageModel
    {
        private readonly ParkeringspladsContext _context;
        private readonly Services.Interfaces.IUser _userService;
        private readonly ISchoolService _schoolService;
        private readonly ICityService _cityService;

        public UpdateUserModel(Services.Interfaces.IUser userService, ParkeringspladsContext context, ISchoolService schoolService, ICityService cityService)
        {
            _context = context;
            _userService = userService;
            _schoolService = schoolService;
            _cityService = cityService;

        }



        [BindProperty]
        public User User { get; set; }
        
        [BindProperty]
        public School School { get; set; }


        [BindProperty]
        public int CityId { get; set; }
        [BindProperty]
        public Address Address { get; set; }
        [BindProperty]
        public UserAddress UserAddress { get; set; }
        [BindProperty]

        public string SchoolName { get; set; }
        [BindProperty]
        public List<SelectListItem> Schools { get; set; }

       
        public List<SelectListItem> City { get; set; }


        private async Task LoadDropdownDataAsync()
        {
            // Fetch the dropdown data
            Schools = await _schoolService.SchoolDropDownAsync();
            City = await _cityService.CityDropDownAsync();
        }

        public async Task<IActionResult> OnGetAsync(int userId)
        {
            await LoadDropdownDataAsync();

            var isAdmin = HttpContext.Session.GetString("IsAdmin");
            var sessionEmail = HttpContext.Session.GetString("UserEmail");
            User user = null;

            if (isAdmin == "true")
            {
                // Admin: Allow selecting from the list of all users
                if (userId > 0)
                {
                    user = await _userService.GetUserAsync(userId);  // Admin can edit any user
                }
            }
            else if (!string.IsNullOrEmpty(sessionEmail))
            {
                // Regular user: Only allow them to update their own profile
                user = await _context.User
                    .Include(u => u.School)
                    .FirstOrDefaultAsync(u => u.Email == sessionEmail);
            }
            else
            {
                return RedirectToPage("/Account/Login/Login");
            }

            // If no user is found, handle the error (admin can go to the user list, regular users should be logged out)
            if (user == null)
            {
                // Handle user not found. For regular users, maybe redirect to their profile page, for admins show a message.
                return string.IsNullOrEmpty(sessionEmail)
                    ? RedirectToPage("/Account/Login/Login")  // Admins go back to user list
                    : RedirectToPage("/Account/Profile");     // Regular users go to their profile
            }


            // If user is found, safely assign the properties
            User = user;  // Only assign the user object if it's not null
            User.Email = user?.Email ?? string.Empty;
            User.FirstName = user?.FirstName ?? string.Empty;
            User.LastName = user?.LastName ?? string.Empty;
            User.Phone = user?.Phone ?? string.Empty;
            User.Title = user?.Title ?? string.Empty;

            // Handle School object if it's null
            School = user?.School;
            SchoolName = user?.School?.SchoolName ?? string.Empty;

            return Page();
        }


        public async Task<IActionResult> OnPostAsync(int userId)
        {
            var isAdmin = HttpContext.Session.GetString("IsAdmin");
            var sessionEmail = HttpContext.Session.GetString("UserEmail");

            User userBeingUpdated;

            if (isAdmin == "true" && userId > 0)
            {
                // Admin can update any user
                userBeingUpdated = await _userService.GetUserAsync(userId);
            }
            else if (!string.IsNullOrEmpty(sessionEmail))
            {
                // Regular user can only update their own profile
                userBeingUpdated = await _context.User.FirstOrDefaultAsync(u => u.Email == sessionEmail);
            }
            else
            {
                return RedirectToPage("/Account/Login/Login");
            }

            if (userBeingUpdated == null)
            {
                return string.IsNullOrEmpty(sessionEmail)
                    ? RedirectToPage("/UserPages/AllUsers")  // Admins go back to user list
                    : RedirectToPage("/Account/Login/Login"); // Regular users go to login
            }

            // Assign the correct UserId to the form-bound User
            User.UserId = userBeingUpdated.UserId;

            bool updateSuccessful = await _userService.UpdateUserAsync(User);

            if (!updateSuccessful)
            {
                ModelState.AddModelError(string.Empty, "The email is already in use by another user.");
                await LoadDropdownDataAsync(); // Re-populate dropdowns
                return Page();
            }

            // Redirect based on admin or regular user
            return string.IsNullOrEmpty(sessionEmail)
                ? RedirectToPage("/UserPages/AllUsers")  // Admins go back to user list
                : RedirectToPage("/Account/Profile");    // Regular users go to their profile
        }


    }
}