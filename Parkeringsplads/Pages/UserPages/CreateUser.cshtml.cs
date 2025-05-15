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


    public Dictionary<string, string> TitleOptions = new()
    {
        {"A", "Administrator" },
        {"P", "Personale" },
        {"S", "Studerende" }
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

    public async Task OnGetAsync()
    {
        // Load dropdown data
        await LoadDropdownDataAsync();
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
                return RedirectToPage("/Account/Login/Login");
            }

            ModelState.AddModelError("User.Email", "Email is already in use.");
        }



        // Repopulate dropdowns if something fails
        await LoadDropdownDataAsync();
        return Page();
    }
}
