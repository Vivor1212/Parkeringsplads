using Parkeringsplads.Models;
using Microsoft.EntityFrameworkCore;
using Parkeringsplads.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Parkeringsplads.Services.EFServices
{
    public class EFCityService : ICityService
    {
        private readonly ParkeringspladsContext _context;

        public EFCityService(ParkeringspladsContext context)
        {
            _context = context;
        }

        public async Task<List<SelectListItem>> CityDropDownAsync()
        {
            return await _context.City
                .Select(s => new SelectListItem
                {
                    Value = s.CityId.ToString(),
                    Text = s.PostalCode + " " + s.CityName
                }).ToListAsync();
        }

        public async Task<List<City>> GetAllCitiesAsync()
        {
            return await _context.City.ToListAsync();
        }

        public async Task<City> GetCityByIdAsync(int cityId)
        {
            return await _context.City.FirstOrDefaultAsync(c => c.CityId == cityId);
        }

        public async Task AddCityAsync(City city)
        {
            try
            {
                Console.WriteLine($"Attempting to add city: {city.CityName}, {city.PostalCode}");

                var exactMatch = await _context.City
                    .AnyAsync(c => c.CityName == city.CityName && c.PostalCode == city.PostalCode);

                if (exactMatch)
                {
                    throw new InvalidOperationException("This city and postal code already exist.");
                }

                var postalCodeConflict = await _context.City
                    .AnyAsync(c => c.PostalCode == city.PostalCode && c.CityName != city.CityName);

                if (postalCodeConflict)
                {
                    throw new InvalidOperationException("This postal code is already assigned to a different city.");
                }

                await _context.City.AddAsync(city);
                await _context.SaveChangesAsync();

                Console.WriteLine("City added successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding city: {ex.Message}");
                throw;
            }
        }

        public async Task UpdateCityAsync(City city)
        {
            _context.City.Update(city);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCityAsync(int cityId)
        {
            var city = await _context.City.FindAsync(cityId);
            if (city != null)
            {
                _context.City.Remove(city);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<City>> GetCitiesAsync(string? searchTerm = null)
        {
            var query = _context.City.AsQueryable();
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var lower = searchTerm.ToLower();
                query = query.Where(c => c.CityName.ToLower().Contains(lower) ||
                c.PostalCode.Contains(lower));
            }

            return await query.ToListAsync();
        }
    }
}
