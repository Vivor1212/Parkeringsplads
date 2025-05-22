using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Parkeringsplads.Models;
using Parkeringsplads.Services.Interfaces;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

public class CreateUserModel : PageModel
{
    private readonly IUser _createUserService;
    private readonly ISchoolService _schoolService;
    private readonly ICityService _cityService;

    public CreateUserModel(IUser createUserService, ISchoolService schoolService,ICityService cityService ,ParkeringspladsContext context)
    {
        _createUserService = createUserService;
        _schoolService = schoolService;
        _cityService = cityService;
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
       
        {"P", "Personale" },
        {"S", "Studerende" },
    };


    public List<SelectListItem> Schools { get; set; }
    public List<SelectListItem> City { get; set; }

    private async Task LoadDropdownDataAsync()
    {
        Schools = await _schoolService.SchoolDropDownAsync();
        City = await _cityService.CityDropDownAsync();
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var isAdmin = HttpContext.Session.GetString("IsAdmin");

        if (isAdmin == "true")
        {
            if (!TitleOptions.ContainsKey("A") || !TitleOptions.ContainsKey("a")) 
            {
                TitleOptions.Add("A", "Admin");
            }
        }

        await LoadDropdownDataAsync();

        return Page();
    }


    public async Task<IActionResult> OnPostAsync()
    {
        if (ModelState.IsValid)
        {
            User.Password = BCrypt.Net.BCrypt.HashPassword(User.Password);

            bool createUser = await _createUserService.CreateUserAsync(User, AddressRoad, AddressNumber, CityId);

            TempData["SuccessMessage"] = "Bruger er nu oprettet. Klar til at logge ind";

            if (createUser)
            {
                var isAdmin = HttpContext.Session.GetString("IsAdmin");

                if (isAdmin == "true")
                {
                    return RedirectToPage("/Admin/AdminDashboard");
                }
                else
                {
                    return RedirectToPage("/Account/Login/Login");
                }
            }

            ModelState.AddModelError("User.Email", "Email er allerede i brug.");
        }

        await LoadDropdownDataAsync();
        return RedirectToPage("/Account/Login/login");
    }

}
