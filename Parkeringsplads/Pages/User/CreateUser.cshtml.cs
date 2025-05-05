
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Parkeringsplads.Models;
using Parkeringsplads.Services.EFServices;

public class CreateUserModel : PageModel
{
    private readonly ICreateUser _createUserService;
    private readonly ParkeringspladsContext _context;
    public CreateUserModel(ICreateUser createUserService, ParkeringspladsContext context)
    {
        _createUserService = createUserService;
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

    public List<SelectListItem> Schools { get; set; }
    public List<SelectListItem> City { get; set; }

    public async Task OnGetAsync()
    {
        await LoadDropdownsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (ModelState.IsValid)
        {
            bool createUser = await _createUserService.CreateUserAsync(User, AddressRoad, AddressNumber, CityId);

            if (createUser)
            {
                return RedirectToPage("/Index");
            }
            else
            {
                ModelState.AddModelError("User.Email", "Email is already in use.");
                await LoadDropdownsAsync();
                return Page();
            }
        }

        await LoadDropdownsAsync();
        return Page();
    }

    private async Task LoadDropdownsAsync()
    {
        Schools = await _context.School
            .Select(s => new SelectListItem
            {
                Value = s.SchoolId.ToString(),
                Text = s.SchoolName
            }).ToListAsync();

        City = await _context.City
            .Select(c => new SelectListItem
            {
                Value = c.CityId.ToString(),
                Text = c.CityName
            }).ToListAsync();
    }
}
