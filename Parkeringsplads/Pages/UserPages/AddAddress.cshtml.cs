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
    public class AddAddressModel : PageModel
    {
        private readonly ParkeringspladsContext _context;
        private readonly Services.Interfaces.IUser _userService;
        private readonly ICityService _cityService;

        public AddAddressModel(Services.Interfaces.IUser userService, ParkeringspladsContext context, ICityService cityService)
        {
            _context = context;
            _userService = userService;
            _cityService = cityService;

        }



        [BindProperty]
        public User User { get; set; }


        [BindProperty]
        public int CityId { get; set; }
        [BindProperty]
        public Address Address { get; set; }
        [BindProperty]
        public UserAddress UserAddress { get; set; }
       

        public List<SelectListItem> City { get; set; }


        private async Task LoadDropdownDataAsync()
        {
            // Fetch the dropdown data
       
            City = await _cityService.CityDropDownAsync();
            // You can similarly fetch City dropdown here if required (using _schoolService or another service).
        }

        public async Task<IActionResult> OnGetAsync(int userId)
        {

            await LoadDropdownDataAsync();

            // Try to get user email from session
            var userEmail = HttpContext.Session.GetString("UserEmail");

            User user;

            if (string.IsNullOrEmpty(userEmail))
            {
                // Admin flow: get user by passed-in userId
                user = await _userService.GetUserAsync(userId);
            }

            else
            {
                // Regular user flow: get user by their session email
                user = await _context.User
                     .Include(u => u.UserAddresses)
           .ThenInclude(ua => ua.Address)
           .ThenInclude(a => a.City)
                    .FirstOrDefaultAsync(u => u.Email == userEmail);
            }
        


            var userSchool = _context.User
                .Include(u => u.School) // Include the School navigation property
                .FirstOrDefault(u => u.Email == userEmail);


            if (user == null)
            {
                // If no user found in the database, redirect to login page
                return RedirectToPage("/Account/Login/Login");
            }

            // Set properties
            User = user;


            return Page(); // Return the Profile page with the user's information
        }

        public async Task<IActionResult> OnPostAsync(int userId)
        {
            var sessionEmail = HttpContext.Session.GetString("UserEmail");

            User userBeingUpdated;

            if (string.IsNullOrEmpty(sessionEmail))
            {
                // Admin flow: use the userId passed from route/form
                userBeingUpdated = await _userService.GetUserAsync(userId);
            }
            else
            {
                // Regular user flow: fetch user using session email
                userBeingUpdated = await _context.User.FirstOrDefaultAsync(u => u.Email == sessionEmail);
            }

            if (userBeingUpdated == null)
            {
                // Redirect admins to user list, regular users to login
                return string.IsNullOrEmpty(sessionEmail)
                    ? RedirectToPage("/UserPages/AllUsers")
                    : RedirectToPage("/Account/Login/Login");
            }

            // Assign the correct UserId to the form-bound User
            User.UserId = userBeingUpdated.UserId;

            // Try to update using service
            bool updateSuccessful = await _userService.UpdateUserAsync(User);

            if (!updateSuccessful)
            {
                ModelState.AddModelError(string.Empty, "The email is already in use by another user.");
                await LoadDropdownDataAsync(); // ensure dropdowns are repopulated
                return Page();
            }

            // ✅ Only update session if user updated their own info
            if (!string.IsNullOrEmpty(sessionEmail) && sessionEmail == userBeingUpdated.Email)
            {
                HttpContext.Session.SetString("UserEmail", User.Email);

                bool isDriver = _context.Driver.Any(d => d.UserId == User.UserId);
                HttpContext.Session.SetString("IsDriver", isDriver ? "true" : "false");
            }

            // Redirect based on who did the update
            return string.IsNullOrEmpty(sessionEmail)
                ? RedirectToPage("/UserPages/AllUsers") // Admins go back to user list
                : RedirectToPage("/Account/Profile");   // Regular users go to profile
        }

    }
}