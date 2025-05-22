using Microsoft.EntityFrameworkCore;
using Parkeringsplads.Models;
using Parkeringsplads.Services.Interfaces;
using System;
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

        public async Task<Trip> CreateTripAsync(Trip trip)
        {
            if(trip == null)
            {
                throw new ArgumentNullException(nameof(trip), "Trip cannot be null");
            }
            _context.Trip.Add(trip);
            await _context.SaveChangesAsync();
            return trip;
        }

        public async Task<List<Trip>> GetAllAvailableTripsAsync(
            string? directionFilter,
            DateTime? dateFilter,
            int? hourFilter,
            string? cityFilter,
            string schoolAddress)
        {
            var query = _context.Trip
                .Include(t => t.Car.Driver)
                    .ThenInclude(d => d.User)
                .Include(t => t.Requests.Where(r => r.RequestStatus == true))
                    .ThenInclude(r => r.Users)
                .OrderBy(t => t.TripDate)
                .ThenBy(t => t.TripTime)
                .AsQueryable();

            if (!string.IsNullOrEmpty(directionFilter))
            {
                if (directionFilter == "ToSchool")
                    query = query.Where(t => t.ToDestination != null && t.ToDestination.ToLower().Contains(schoolAddress.ToLower()));
                else if (directionFilter == "FromSchool")
                    query = query.Where(t => t.FromDestination != null && t.FromDestination.ToLower().Contains(schoolAddress.ToLower()));
            }

            if (dateFilter.HasValue)
            {
                query = query.Where(t => t.TripDate == DateOnly.FromDateTime(dateFilter.Value));
            }

            if (hourFilter.HasValue)
            {
                query = query.Where(t => t.TripTime.Hour == hourFilter.Value);
            }

            if (!string.IsNullOrEmpty(cityFilter))
            {
                string cityLower = cityFilter.ToLower();

                if (directionFilter == "Til Skole")
                {
                    query = query.Where(t =>
                        !string.IsNullOrEmpty(t.FromDestination) &&
                        t.FromDestination.ToLower().Contains(cityLower));
                }
                else if (directionFilter == "Fra Skole")
                {
                    query = query.Where(t =>
                        !string.IsNullOrEmpty(t.ToDestination) &&
                        t.ToDestination.ToLower().Contains(cityLower));
                }
                else
                {
                    query = query.Where(t =>
                        (!string.IsNullOrEmpty(t.FromDestination) && t.FromDestination.ToLower().Contains(cityLower)) ||
                        (!string.IsNullOrEmpty(t.ToDestination) && t.ToDestination.ToLower().Contains(cityLower)));
                }
            }

            return await query.ToListAsync();
        }

        public async Task<Trip?> GetTripByIdAsync(int tripId)
        {
            return await _context.Trip
                .Include(t => t.Car.Driver)
                    .ThenInclude(d => d.User)
                .Include(t => t.Requests.Where(r => r.RequestStatus == true))
                    .ThenInclude(r => r.Users)
                .FirstOrDefaultAsync(t => t.TripId == tripId);
        }


        public async Task AdminDeleteTripAsync(int tripId)
        {
            var trip = await _context.Trip
                .Include(t => t.Requests)
                .FirstOrDefaultAsync(t => t.TripId == tripId);

            if (trip == null)
            {
                throw new ArgumentException($"Trip with ID {tripId} not found");
            }

            _context.Request.RemoveRange(trip.Requests);

            _context.Trip.Remove(trip);

            await _context.SaveChangesAsync();
        }
        public async Task<bool> DeleteTripAsync(int tripId, int userId)
        {
            var trip = await _context.Trip.Include(t => t.Requests).Include(t => t.Car).ThenInclude(c => c.Driver).FirstOrDefaultAsync(t => t.TripId == tripId && t.Car.Driver.UserId == userId);
            if (trip == null)
            {
                return false;
            }

            _context.Trip.Remove(trip);
            await _context.SaveChangesAsync();
            return true;

        }
    }
}
