using Parkeringsplads.Models;
using Parkeringsplads.Services.Interfaces;
using System.Threading.Tasks;

namespace Parkeringsplads.Services.EFServices
{
    public class EFTripService : ITripService
    {
        private readonly ParkeringspladsContext _context;

        public EFTripService(ParkeringspladsContext context)
        {
            _context = context;
        }

        public async Task CreateTripAsync(Trip trip)
        {
            _context.Trip.Add(trip);
            await _context.SaveChangesAsync();
        }
    }
}
