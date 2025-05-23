using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using Parkeringsplads.Models;
using Parkeringsplads.Services.EFServices;
using Parkeringsplads.Services.Interfaces;

public class AddAddressModel : PageModel
{
    private readonly ICityService _cityService;
    private readonly IAddressService _addressService;
    private readonly IUser _userService;



    public AddAddressModel(ICityService cityService, IAddressService addressService, IUser userService)
    {
        _addressService = addressService;
        _cityService = cityService;
        _userService = userService;

    }

    [BindProperty]
    public Address Address { get; set; }

   
   
    [BindProperty]
    public int CityId { get; set; }

    [BindProperty]
    public int UserId { get; set; }

    public List<SelectListItem> UserList { get; set; } = new();


    public List<SelectListItem> City { get; set; }


    private async Task LoadDropdownDataAsync()
    {
        City = await _cityService.CityDropDownAsync();
    }


    public async Task<IActionResult> OnGetAsync(int? selectedUserId = null)
    {
        // Fetch city data for dropdown
        City = await _cityService.CityDropDownAsync();

        // Check if the user is an admin
        if (HttpContext.Session.GetString("IsAdmin") == "true")
        {
            // Fetch all users for admin to select from
            var users = await _userService.GetAllUsersAsync();
            UserList = users.Select(u => new SelectListItem
            {
                Value = u.UserId.ToString(),
                Text = $"{u.FirstName} {u.LastName} ({u.Email})"
            }).ToList();

            // If a selectedUserId is provided, set the UserId
            if (selectedUserId.HasValue)
            {
                UserId = selectedUserId.Value;
            }
        }
        else
        {
            // Validate if the user is logged in (check session data)
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToPage("./Login/Login"); // Redirect if no user is logged in
            }

            var user = await _userService.GetUserAsync(userId.Value); // Get user by userId from the session

            if (user == null)
            {
                return RedirectToPage("./Login/Login"); // Redirect if the user does not exist
            }

            UserId = user.UserId; // Set the userId based on the valid user
        }

        // Return the page after performing the necessary checks
        return Page();
    }






    public async Task<IActionResult> OnPostAsync()
    {
        var address = await _addressService.GetOrCreateAddressAsync(
            Address.AddressRoad, Address.AddressNumber, CityId);

        await _addressService.LinkAddressToUserAsync(UserId, address.AddressId);

        TempData["SuccessMessage"] = "Adresse blev tilføjet.";

        if (HttpContext.Session.GetString("IsAdmin") == "true")
        {
            return RedirectToPage("/Admin/AdminDashboard");
        }

        return RedirectToPage("/Account/Profile");
    }



}
