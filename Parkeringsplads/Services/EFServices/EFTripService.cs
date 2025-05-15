using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parkeringsplads.Models;
using Parkeringsplads.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
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

        public async Task<List<Trip>> GetAllAvailableTripsAsync(
            string? directionFilter,
            DateTime? dateFilter,
            int? hourFilter,
            string schoolAddress)
        {
            var query = _context.Trip
                .Include(t => t.Driver)
                    .ThenInclude(d => d.User)
                .Include(t => t.Requests.Where(r => r.RequestStatus == true))
                    .ThenInclude(r => r.Users)
                .OrderBy(t => t.TripDate)
                .ThenBy(t => t.TripTime)
                .AsQueryable();

            if (!string.IsNullOrEmpty(directionFilter) && !string.IsNullOrEmpty(schoolAddress))
            {
                if (directionFilter == "ToSchool")
                {
                    query = query.Where(t => t.ToDestination == schoolAddress);
                }
                else if (directionFilter == "FromSchool")
                {
                    query = query.Where(t => t.FromDestination == schoolAddress);
                }
            }

            if (dateFilter.HasValue)
            {
                query = query.Where(t => t.TripDate == DateOnly.FromDateTime(dateFilter.Value));
            }

            if (hourFilter.HasValue)
            {
                query = query.Where(t => t.TripTime.Hour == hourFilter.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<Trip?> GetTripByIdAsync(int tripId)
        {
            return await _context.Trip
                .Include(t => t.Driver)
                    .ThenInclude(d => d.User)
                .Include(t => t.Requests.Where(r => r.RequestStatus == true))
                    .ThenInclude(r => r.Users)
                .FirstOrDefaultAsync(t => t.TripId == tripId);
        }
    }
}