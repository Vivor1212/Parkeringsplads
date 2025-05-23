using Microsoft.AspNetCore.Mvc.Rendering;
using Parkeringsplads.Models;
namespace Parkeringsplads.Services.Interfaces
{
    public interface ICityService
    {
        Task<List<City>> GetAllCitiesAsync();
        Task<List<City>> GetCitiesAsync(string? searchTerm = null);
        Task<City> GetCityByIdAsync(int cityId);
        Task AddCityAsync(City city);
        Task UpdateCityAsync(City city); 
        Task DeleteCityAsync(int cityId);
        Task<List<SelectListItem>> CityDropDownAsync(); 
    }
}
