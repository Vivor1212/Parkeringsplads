using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Parkeringsplads.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class CreateUserModel : PageModel
{
    private readonly ParkeringspladsContext _context;

    public CreateUserModel(ParkeringspladsContext context)
    {
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
        // Populate the dropdown lists for schools and cities
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

    public async Task<IActionResult> OnPostAsync()
    {
        if (ModelState.IsValid)
        {
            // 1. Check if the address already exists
            var existingAddress = await _context.Address
                .FirstOrDefaultAsync(a => a.AddressRoad == AddressRoad && a.AddressNumber == AddressNumber && a.CityId == CityId);

            int addressId;

            // If the address doesn't exist, create a new one
            if (existingAddress == null)
            {
                var newAddress = new Address
                {
                    AddressRoad = AddressRoad,
                    AddressNumber = AddressNumber,
                    CityId = CityId
                };
                _context.Address.Add(newAddress);
                await _context.SaveChangesAsync();
                addressId = newAddress.AddressId;
            }
            else
            {
                // If the address exists, use the existing one
                addressId = existingAddress.AddressId;
            }

            // 2. Create the user and link them to the address through UserAddress
            _context.User.Add(User);
            await _context.SaveChangesAsync();

            // 3. Create the UserAddress linking table entry
            var userAddress = new UserAddress
            {
                User_Id = User.UserId,
                Address_Id = addressId
            };
            _context.UserAddress.Add(userAddress);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Index");
        }

        // debug log for console
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
        if (!ModelState.IsValid)
        {
            foreach (var modelState in ModelState)
            {
                var key = modelState.Key;
                var errors = modelState.Value.Errors;

                foreach (var error in errors)
                {
                    Console.WriteLine($"Validation error in '{key}': {error.ErrorMessage}");
                }
            }

            // Re-populate dropdowns
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

            return RedirectToPage("/Index");

        }

        return RedirectToPage("/Index");

    }
}
