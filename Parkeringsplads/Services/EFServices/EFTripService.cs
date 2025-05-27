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
            if (trip == null)
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

            var today = DateOnly.FromDateTime(DateTime.Today);
            var now = TimeOnly.FromDateTime(DateTime.Now);
            query = query.Where(t =>
                t.TripDate > today || (t.TripDate == today && t.TripTime >= now));


            if (!string.IsNullOrEmpty(directionFilter))
            {
                var schoolAddressLower = schoolAddress.ToLower();

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

            if (!string.IsNullOrEmpty(cityFilter))
            {
                string cityLower = cityFilter.ToLower();

                if (directionFilter == "ToSchool")
                {
                    query = query.Where(t =>
                        !string.IsNullOrEmpty(t.FromDestination) &&
                        t.FromDestination.ToLower().Contains(cityLower));
                }
                else if (directionFilter == "FromSchool")
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
                .Include(t => t.Car)
                    .ThenInclude(c => c.Driver)
                        .ThenInclude(d => d.User)
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
        public async Task<OperationResult> DeleteTripAsync(int tripId, int userId)
        {
            var trip = await _context.Trip.Include(t => t.Car).ThenInclude(c => c.Driver).FirstOrDefaultAsync(t => t.TripId == tripId && t.Car != null && t.Car.Driver != null && t.Car.Driver.UserId == userId);
            if (trip == null)
            {
                return new OperationResult
                {
                    Success = false,
                    Message = "Turen blev ikke fundet, eller du har ikke tilladelse til at slette den."
                };
            }

            _context.Trip.Remove(trip);
            await _context.SaveChangesAsync();
            return new OperationResult
            {
                Success = true,
                Message = "Turen blev slettet."
            };

        }

        public async Task<TripValidation> GetDriverTripAsync(int tripId, int userId)
        {
            var trip = await _context.Trip.Include(t => t.Requests).ThenInclude(r => r.Users)
                                          .Include(t => t.Car).ThenInclude(c => c.Driver)
                                          .FirstOrDefaultAsync(t => t.TripId == tripId && t.Car != null && t.Car.Driver != null && t.Car.Driver.UserId == userId);
            if (trip == null)
            {
                return new TripValidation
                {
                    IsValid = false,
                    RedirectPage = "./TripPages/DriversTrips",
                    ErrorMessage = "Rejse blev ikke fundet eller du har ikke adgang til den."
                };
            }

            return new TripValidation
            {
                IsValid = true,
                Trip = trip
            };
        }

        public async Task<IEnumerable<Car>> GetDriversCarsAsync(int driverId)
        {
            return await _context.Car.Where(c => c.DriverId == driverId).ToListAsync();
        }

        public async Task<IEnumerable<Trip>> GetDriversFutureTripsAsync(int userId)
        {
            return await _context.Trip.Include(t => t.Requests).Include(t => t.Car).ThenInclude(c => c.Driver)
                .Where(t => t.Car.Driver.UserId == userId && (t.TripDate > DateOnly.FromDateTime(DateTime.Today) || (t.TripDate == DateOnly.FromDateTime(DateTime.Today) && t.TripTime >= TimeOnly.FromDateTime(DateTime.Now)))).ToListAsync();
        }

        public async Task<List<Trip>> GetAllTripsOnUserAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            var trips = await _context.Trip
                .Include(t => t.Car.Driver).ThenInclude(d => d.User)
                .Include(t => t.Requests)
                .Where(t => t.Requests.Any(r => r.UserId == user.UserId && r.RequestStatus == true))
                .OrderBy(t => t.TripDate)
                .ThenBy(t => t.TripTime)
                .ToListAsync();

            return trips;
        }


        public async Task<Driver?> GetDriverWithCarsByEmailAsync(string email)
        {
            return await _context.Driver.Include(d => d.Cars).FirstOrDefaultAsync(d => d.User.Email == email);
        }

        public async Task<List<Trip>> GetTripsWithDriverAsync(string? searchTerm = null)
        {
            var query = _context.Trip.Include(t => t.Car).ThenInclude(c => c.Driver).ThenInclude(d => d.User).AsQueryable();
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var lower = searchTerm.ToLower();
                query = query.Where(t => t.FromDestination.ToLower().Contains(lower) ||
                t.ToDestination.ToLower().Contains(lower) ||
                t.TripDate.ToString().Contains(lower) ||
                t.TripTime.ToString().Contains(lower) ||
                t.TripSeats.ToString().Contains(lower) ||
                t.Car.Driver.DriverLicense.ToLower().Contains(lower) ||
                t.Car.Driver.User.FirstName.ToLower().Contains(lower) ||
                t.Car.Driver.User.LastName.ToLower().Contains(lower));
            }

            return await query.ToListAsync();
        }

        public async Task<List<Trip>> GetTripsWithDetailsAsync()
        {
            return await _context.Trip.Include(t => t.Car).ThenInclude(c => c.Driver).ThenInclude(d => d.User).OrderByDescending(t => t.TripDate).ThenBy(t => t.TripTime).ToListAsync();
        }

        public async Task<List<Trip>> GetAllTripsForDriverAsync(int userId)
        {
            return await _context.Trip
                .Include(t => t.Requests)
                .Include(t => t.Car)
                    .ThenInclude(c => c.Driver)
                .Where(t => t.Car.Driver.UserId == userId)
                .OrderBy(t => t.TripDate)
                .ThenBy(t => t.TripTime)
                .ToListAsync();

        }


        public string ExtractCity(string? address)
        {
            if (string.IsNullOrWhiteSpace(address))
                return "";

            var parts = address.Split(',');
            return parts.Length >= 1 ? parts[parts.Length - 1] : address;
        }
    }
}
