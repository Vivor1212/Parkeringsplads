﻿using Microsoft.AspNetCore.Mvc;
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
        private readonly Services.Interfaces.IUser _userService;
        private readonly ISchoolService _schoolService;
        private readonly ICityService _cityService;

        public UpdateUserModel(Services.Interfaces.IUser userService, ISchoolService schoolService, ICityService cityService)
        {
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

        public Dictionary<string, string> TitleOptions = new()
    {

        {"P", "Personale" },
        {"S", "Studerende" }
    };
        public List<SelectListItem> City { get; set; }

        public int SelectedUserId { get; set; }


        private async Task LoadDropdownDataAsync()
        {
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
                var users = await _userService.GetAllUsersAsync();
                ViewData["Users"] = users;

                if (userId > 0)
                {
                    user = await _userService.GetUserWithDetailsByIdAsync(userId);
                }
            }
            else if (!string.IsNullOrEmpty(sessionEmail))
            {
                user = await _userService.GetUserWithDetailsByEmailAsync(sessionEmail);
            }

            if (user == null)
            {
                return string.IsNullOrEmpty(sessionEmail)
                    ? RedirectToPage("/Account/Login/Login")
                    : RedirectToPage("/Account/Profile");
            }
           
            User = user;
            School = user.School;
            SchoolName = user.School?.SchoolName;

            var userAddress = user.UserAddresses?.FirstOrDefault();
            if (userAddress != null)
            {
                Address = userAddress.Address;
                CityId = userAddress.Address.CityId;
            }

            return Page(); 
        }

        public async Task<IActionResult> OnPostAsync(int userId)
        {
            var isAdmin = HttpContext.Session.GetString("IsAdmin");
            var sessionEmail = HttpContext.Session.GetString("UserEmail");

            //if (!ModelState.IsValid)
            //{
            //    TempData["ErrorMessage"] = "Ugyldige data indsendt: " + string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            //    await LoadDropdownDataAsync();
            //    return Page();
            //}

            User userBeingUpdated;

            try
            {
                if (isAdmin == "true" && userId > 0)
                {
                    userBeingUpdated = await _userService.GetUserAsync(userId);
                }
                else if (!string.IsNullOrEmpty(sessionEmail))
                {
                    userBeingUpdated = await _userService.GetUserWithDetailsByEmailAsync(sessionEmail);
                }
                else
                {
                    return RedirectToPage("/Account/Login/Login");
                }

                if (userBeingUpdated == null)
                {
                    return string.IsNullOrEmpty(sessionEmail)
                        ? RedirectToPage("/UserPages/AllUsers")
                        : RedirectToPage("/Account/Login/Login");
                }

                User.UserId = userBeingUpdated.UserId;

                bool updateSuccessful = await _userService.UpdateUserAsync(User);

                if (updateSuccessful)
                {
                    TempData["SuccessMessage"] = "Bruger opdateret";

                    if (isAdmin == "true")
                    {
                        return RedirectToPage("/Admin/AdminDashboard");
                    }
                    else
                    {
                        return RedirectToPage("/Account/Profile");
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Der skete en fejl, prøv igen.";
                    await LoadDropdownDataAsync();
                    return Page(); 
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Fejl ved opdatering af bruger: " + ex.Message;
                await LoadDropdownDataAsync();
                return Page(); 
            }
        }
    }
}