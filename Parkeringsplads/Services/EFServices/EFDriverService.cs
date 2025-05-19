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
    }
}
