using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Parkeringsplads.Models;
using Parkeringsplads.Services.Interfaces;
using System.ComponentModel.DataAnnotations;

public class CreateUserModel : PageModel
{
    private readonly IUser _createUserService;
    private readonly ISchoolService _schoolService;
    private readonly ICityService _cityService;
    private readonly ParkeringspladsContext _context;

    public CreateUserModel(IUser createUserService, ISchoolService schoolService,ICityService cityService ,ParkeringspladsContext context)
    {
        _createUserService = createUserService;
        _schoolService = schoolService;
        _cityService = cityService;
        _context = context;
    }

    [BindProperty]
    public User User { get; set; }


    [BindProperty]
    public string AddressRoad { get; set; }

    [BindProperty]
    public string AddressNumber { get; set; }

    [BindProperty]
    public int CityId { get; set; }
    public string ErrorMessage { get; set; }


    public Dictionary<string, string> TitleOptions = new()
    {
       
        {"P", "Personale" },
        {"S", "Studerende" },
    };


    public List<SelectListItem> Schools { get; set; }
    public List<SelectListItem> City { get; set; }

    // This method loads schools and cities
    private async Task LoadDropdownDataAsync()
    {
        // Fetch the dropdown data
        Schools = await _schoolService.SchoolDropDownAsync();
        // You can similarly fetch City dropdown here if required (using _schoolService or another service).
        City = await _cityService.CityDropDownAsync();
    }

    public async Task<IActionResult> OnGetAsync()
    {
        // Check if the user is an admin
        var isAdmin = HttpContext.Session.GetString("IsAdmin");

        // If user is an admin, add the "Admin" role to the options
        if (isAdmin == "true")
        {
            if (!TitleOptions.ContainsKey("A")) // Ensure we don't add it multiple times
            {
                TitleOptions.Add("A", "Admin");
            }
        }

        // If the user is not an admin, show an error message and stay on the current page
        if (isAdmin != "true")
        {
            ErrorMessage = "Du har ikke tilladelse til at oprette en bruger. Kun administratorer kan oprette brugere.";
        }

        // Load dropdown data (schools and cities)
        await LoadDropdownDataAsync();

        // Return the page to render the UI (with or without the error message)
        return Page();
    }


    public async Task<IActionResult> OnPostAsync()
    {
        if (ModelState.IsValid)
        {
            // Hash the user's password before saving
            User.Password = BCrypt.Net.BCrypt.HashPassword(User.Password);

            // Call the service to create the user
            bool createUser = await _createUserService.CreateUserAsync(User, AddressRoad, AddressNumber, CityId);

            if (createUser)
            {
                // Check if the current user is an admin
                var isAdmin = HttpContext.Session.GetString("IsAdmin");

                if (isAdmin == "true")
                {
                    // Redirect to the Admin Dashboard if the user is an admin
                    return RedirectToPage("/Admin/AdminDashboard");
                }
                else
                {
                    // Redirect to the login page if the user is not an admin
                    return RedirectToPage("/Account/Login/Login");
                }
            }

            // If creating the user fails, add an error message
            ModelState.AddModelError("User.Email", "Email is already in use.");
        }

        // Repopulate dropdowns if something fails
        await LoadDropdownDataAsync();
        return Page();
    }

}
