using Parkeringsplads.Models;
using Parkeringsplads.Services.Interfaces;

namespace Parkeringsplads.Services.EFServices
{
    public class EFTripService : ITripService
    {
        private readonly ParkeringspladsContext _context;
        public EFTripService(ParkeringspladsContext context)
        {
            _context = context;
        }
        public void CreateTrip(Trip trip)
        {
            _context.Trip.Add(trip);
            _context.SaveChanges();
        }
    }
}
