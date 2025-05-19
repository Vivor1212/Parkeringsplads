using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Parkeringsplads.Models;
using Parkeringsplads.Services.Interfaces;

namespace Parkeringsplads.Pages.Admin;

public class CityAdmin : PageModel
{
    private readonly ICityService _cityService;

    [BindProperty(SupportsGet = true)]
    public int CityId { get; set; }

    [BindProperty]
    public string CityName { get; set; }

    [BindProperty]
    public string PostalCode { get; set; }

    public CityAdmin(ICityService cityService)
    {
        _cityService = cityService;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var isAdmin = HttpContext.Session.GetString("IsAdmin");

        if (string.IsNullOrEmpty(isAdmin) || isAdmin != "true")
        {
            return RedirectToPage("/Admin/NotAdmin");
        }

        if (CityId != 0)
        {
            var city = await _cityService.GetCityByIdAsync(CityId);
            if (city != null)
            {
                CityName = city.CityName;
                PostalCode = city.PostalCode;
            }
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (CityId == 0)
        {
            var city = new City
            {
                CityName = CityName,
                PostalCode = PostalCode
            };

            await _cityService.AddCityAsync(city);
        }
        else 
        {
            var city = await _cityService.GetCityByIdAsync(CityId);
            if (city == null)
            {
                ModelState.AddModelError("", "City not found.");
                return Page();
            }

            city.CityName = CityName;
            city.PostalCode = PostalCode;

            await _cityService.UpdateCityAsync(city);
        }

        return RedirectToPage("/Admin/Admindashboard");
    }
}
