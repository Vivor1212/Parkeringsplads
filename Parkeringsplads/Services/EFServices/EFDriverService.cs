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
    }
}
