using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Parkeringsplads.Models;
using Parkeringsplads.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Parkeringsplads.Pages.Admin
{
    public class CityModel : PageModel
    {
        private readonly ICityService _cityService;
        private readonly ILogger<CityModel> _logger;

        [BindProperty]
        public string CityName { get; set; }

        [BindProperty]
        public string PostalCode { get; set; }

        public CityModel(ICityService cityService, ILogger<CityModel> logger)
        {
            _cityService = cityService;
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        public string CitySearch { get; set; }

        public List<City> MatchingCities { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {

            var isAdmin = HttpContext.Session.GetString("IsAdmin");

            if (string.IsNullOrEmpty(isAdmin) || isAdmin != "true")
            {
                // User is not an admin, redirect to login
                return RedirectToPage("/Admin/NotAdmin");
            }

            if (!string.IsNullOrEmpty(CitySearch))
            {
                MatchingCities = (await _cityService.GetAllCitiesAsync())
                    .Where(city => city.CityName.Contains(CitySearch, StringComparison.OrdinalIgnoreCase) ||
                                   city.PostalCode.Contains(CitySearch, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            else
            {
                MatchingCities = new List<City>();
            }
            return Page();
        }

        public async Task<JsonResult> OnGetSearchCitiesAsync(string term)
        {
            var allCities = await _cityService.GetAllCitiesAsync();

            var results = allCities
                .Where(c => c.CityName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                            c.PostalCode.Contains(term, StringComparison.OrdinalIgnoreCase))
                .Select(c => new { id = c.CityId, name = c.CityName, postalCode = c.PostalCode })
                .ToList();

            return new JsonResult(results);
        }

        public async Task<IActionResult> OnPostAddCityAsync()
        {
            if (string.IsNullOrWhiteSpace(CityName) || string.IsNullOrWhiteSpace(PostalCode))
            {
                ModelState.AddModelError(string.Empty, "City Name and Postal Code must not be empty.");
                return Page();
            }

            var newCity = new City
            {
                CityName = CityName,
                PostalCode = PostalCode
            };

            try
            {
                await _cityService.AddCityAsync(newCity);
                return RedirectToPage();
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                return Page();
            }
            
        }

        // Request model for deletion
        public class DeleteCityRequest
        {
            public int CityId { get; set; }
        }

        // Handles JSON POST body: { "cityId": 123 }
        public async Task<IActionResult> OnPostDeleteCityAsync(int cityId)
        {
            try
            {
                var city = await _cityService.GetCityByIdAsync(cityId);
                if (city == null)
                {
                    ModelState.AddModelError("", "City not found.");
                    return Page();
                }

                await _cityService.DeleteCityAsync(cityId);
                return RedirectToPage(); // Refresh after delete
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting city.");
                ModelState.AddModelError("", "Error deleting city.");
                return Page();
            }
        }

    }
}
