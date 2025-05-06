using Parkeringsplads.Models;
namespace Parkeringsplads.Interfaces
{
    public interface ICityService
    {
        Task<List<City>> GetAllCitiesAsync();         // Get all cities
        Task<City> GetCityByIdAsync(int cityId);      // Get a specific city by ID
        Task AddCityAsync(City city);                  // Add a new city
        Task UpdateCityAsync(City city);               // Update an existing city
        Task DeleteCityAsync(int cityId);              // Delete a city by ID
    }
}
