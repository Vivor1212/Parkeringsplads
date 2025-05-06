using Parkeringsplads.Interfaces;
using Parkeringsplads.Models;
using Microsoft.EntityFrameworkCore;

namespace Parkeringsplads.Services
{
    public class CityService : ICityService
    {
        private readonly TestParkeringspladsContext _context;

        // Constructor injection for the DbContext
        public CityService(TestParkeringspladsContext context)
        {
            _context = context;
        }

        // Get all cities
        public async Task<List<City>> GetAllCitiesAsync()
        {
            return await _context.Cities.ToListAsync();
        }

        // Get a specific city by ID
        public async Task<City> GetCityByIdAsync(int cityId)
        {
            return await _context.Cities.FirstOrDefaultAsync(c => c.CityId == cityId);
        }

        // Add a new city
        public async Task AddCityAsync(City city)
        {
            try
            {
                Console.WriteLine($"Attempting to add city: {city.CityName}, {city.PostalCode}");

                // Check for duplicates
                var exactMatch = await _context.Cities
                    .AnyAsync(c => c.CityName == city.CityName && c.PostalCode == city.PostalCode);

                if (exactMatch)
                {
                    throw new InvalidOperationException("This city and postal code already exist.");
                }

                // Check for postal code conflict
                var postalCodeConflict = await _context.Cities
                    .AnyAsync(c => c.PostalCode == city.PostalCode && c.CityName != city.CityName);

                if (postalCodeConflict)
                {
                    throw new InvalidOperationException("This postal code is already assigned to a different city.");
                }

                // Add to the database
                await _context.Cities.AddAsync(city);
                await _context.SaveChangesAsync();

                Console.WriteLine("City added successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding city: {ex.Message}");
                throw;
            }
        }

        // Update an existing city
        public async Task UpdateCityAsync(City city)
        {
            _context.Cities.Update(city);
            await _context.SaveChangesAsync();
        }

        // Delete a city by ID
        public async Task DeleteCityAsync(int cityId)
        {
            var city = await _context.Cities.FindAsync(cityId);
            if (city != null)
            {
                _context.Cities.Remove(city);
                await _context.SaveChangesAsync();
            }
        }
    }
}
