using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using Parkeringsplads.Models;
using Parkeringsplads.Services.EFServices;
using Parkeringsplads.Services.Interfaces;

public class AddAddressModel : PageModel
{
    private readonly ParkeringspladsContext _context;
    private readonly ICityService _cityService;
    private readonly IAddressService _addressService;


    public AddAddressModel(ParkeringspladsContext context, ICityService cityService, IAddressService addressService)
    {
        _context = context;
        _addressService = addressService;
        _cityService = cityService;

    }

    [BindProperty]
    public Address Address { get; set; }

   
   
    [BindProperty]
    public int CityId { get; set; }

    [BindProperty]
    public int UserId { get; set; }

    public List<SelectListItem> City { get; set; }

    private async Task LoadDropdownDataAsync()
    {
        // Fetch the dropdown data
   
        City = await _cityService.CityDropDownAsync();
    }


    public async Task<IActionResult> OnGetAsync(int userId)
    {

        UserId = userId;
        City = await _cityService.CityDropDownAsync();
      
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var address = await _addressService.GetOrCreateAddressAsync(
            Address.AddressRoad, Address.AddressNumber, CityId);

        await _addressService.LinkAddressToUserAsync(UserId, address.AddressId);

        TempData["SuccessMessage"] = "Adresse blev tilføjet.";
        return RedirectToPage("/Account/Profile");
    }
}
