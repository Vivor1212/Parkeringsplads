using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Parkeringsplads.Models;
using Parkeringsplads.Services.Interfaces;

namespace Parkeringsplads.Services.EFServices
{
    public class EFDriverService : IDriverService
    {
        private readonly ParkeringspladsContext _context;

        public EFDriverService(ParkeringspladsContext context)
        {
            _context = context;
        }

        public async Task<Driver> CreateDriverAsync(Driver driver)
        {
            if (driver == null)
            {
                throw new ArgumentNullException(nameof(driver));
            }

            await _context.Driver.AddAsync(driver);
            await _context.SaveChangesAsync();
            return driver;
        }

        public async Task<Driver?> GetDriverByUserIdAsync(int userId)
        {
            return await _context.Driver.FirstOrDefaultAsync(d => d.UserId == userId);
        }

        public async Task<List<SelectListItem>> GetAllDriversAsync()
        {
            return await _context.Driver
                .Include(d => d.User)
                .Select(d => new SelectListItem
                {
                    Value = d.DriverId.ToString(),
                    Text = d.User.FirstName + " " + d.User.LastName
                })
                .ToListAsync();
        }

        public async Task DeleteDriverAsync(int driverId)
        {
            var driver = await _context.Driver
                .FirstOrDefaultAsync(d => d.DriverId == driverId);

            if (driver == null)
            {
                throw new KeyNotFoundException("Driver not found.");
            }

            _context.Driver.Remove(driver);

            await _context.SaveChangesAsync();
        }

        public async Task<Driver?> GetDriverByUserIdAsync(int userId)
        {
            return await _context.Driver.FirstOrDefaultAsync(d => d.UserId == userId);
        }

        public async Task<bool> UnbecomeDriver(int userId)
        {
            var driver = await _context.Driver.FirstOrDefaultAsync(d => d.UserId == userId);
            if (driver == null)
                return false;

            _context.Driver.Remove(driver);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DriverExistsAsync(int userId)
        {
            return await _context.Driver.AnyAsync(d => d.UserId == userId);
        }

        public async Task<Driver?> GetDriverWithUserAsync(int driverId)
        {
            return await _context.Driver.Include(d => d.User).FirstOrDefaultAsync(d => d.DriverId == driverId);
        }

        public async Task<bool> UpdateDriverAsync(int driverId, string driverLicense, string driverCpr, int userId)
        {
            var driver = await _context.Driver.FirstOrDefaultAsync(d => d.DriverId == driverId);
            if (driver == null)
            {
                return false;
            }

            driver.DriverLicense = driverLicense;
            driver.DriverCpr = driverCpr;
            driver.UserId = userId;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Driver>> GetDriversWithUserAsync(string? searchTerm = null)
        {
            var query = _context.Driver.Include(d => d.User).AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var lower = searchTerm.ToLower();
                query = query.Where(d => d.DriverId.ToString().Contains(searchTerm) ||
                d.DriverLicense.ToLower().Contains(lower) ||
                d.DriverCpr.ToLower().Contains(lower) ||
                d.User.FirstName.ToLower().Contains(searchTerm.ToLower()) ||
                d.User.LastName.ToLower().Contains(searchTerm.ToLower()) ||
                d.User.Email.ToLower().Contains(lower));
            }

            return await query.ToListAsync();
        }

        public async Task<Driver?> GetDriverWithDetailsAsync(int driverId)
        {
            return await _context.Driver.Include(d => d.Cars).Include(d => d.User).ThenInclude(u => u.School).ThenInclude(s => s.Address).ThenInclude(a => a.City).Include(d => d.User).ThenInclude(u => u.UserAddresses).ThenInclude(ua => ua.Address).ThenInclude(a => a.City).FirstOrDefaultAsync(d => d.DriverId == driverId);
        }

        public async Task<DriverValidation> ValidateAndCreateDriverAdync(Driver driver)
        {
            if (driver.UserId == 0)
            {
                return new DriverValidation
                {
                    IsValid = false,
                    ErrorMessage = "Vælg en bruger for chaufføren."
                };
            }

            var user = await _context.User.FindAsync(driver.UserId);
            if (user == null)
            {
                return new DriverValidation
                {
                    IsValid = false,
                    ErrorMessage = "Brugeren blev ikke fundet."
                };
            }

            var existingDriver = await _context.Driver.FirstOrDefaultAsync(d => d.UserId == driver.UserId);
            if (existingDriver != null)
            {
                return new DriverValidation
                {
                    IsValid = false,
                    ErrorMessage = "Brugeren er allerede tilknyttet som chauffør."
                };
            }

            _context.Driver.Add(driver);
            await _context.SaveChangesAsync();

            return new DriverValidation
            {
                IsValid = true,
                Driver = driver
            };
        }
    }
}
