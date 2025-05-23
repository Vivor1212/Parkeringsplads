using Microsoft.EntityFrameworkCore;
using Parkeringsplads.Models;
using Parkeringsplads.Services.Interfaces;
using System.Threading.Tasks;

namespace Parkeringsplads.Services.EFServices
{
    public class EFRequestService : IRequestService
    {
        private readonly ParkeringspladsContext _context;

        public EFRequestService(ParkeringspladsContext context)
        {
            _context = context;
        }

        public async Task<List<Request>> GetAllRequestsForUser(User user)
        {
            return await _context.Request.Include(r => r.Trip).ThenInclude(t => t.Car).ThenInclude(c => c.Driver).ThenInclude(d => d.User).Include(r => r.Users).Where(r => r.UserId == user.UserId).ToListAsync();
        }

        public async Task DeleteRequestAsync(int requestId)
        {
            var request = await _context.Request.Include(r => r.Trip).FirstOrDefaultAsync(r => r.RequestId == requestId);
            if (request != null)
            {
                if (request.RequestStatus == true)
                {
                    request.Trip.TripSeats++;
                }

                _context.Request.Remove(request);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Request> GetRequestByIdAsync(int requestId)
        {
            return await _context.Request
                .Include(r => r.Trip)
                    .ThenInclude(t => t.Car.Driver)
                        .ThenInclude(d => d.User)
                .FirstOrDefaultAsync(r => r.RequestId == requestId);
        }


        public async Task<Request> CreateRequestAsync(Request request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "Request cannot be null");
            }
            _context.Request.Add(request);
            await _context.SaveChangesAsync();
            return request;
        }


        public async Task<OperationResult> AcceptRequestAsync(int requestId, int tripId)
        {
            var trip = await _context.Trip
                .Include(t => t.Requests)
                .Include(t => t.Car)
                    .ThenInclude(c => c.Driver)
                .FirstOrDefaultAsync(t => t.TripId == tripId);

            if (trip == null)
            {
                return new OperationResult { Success = false, Message = "Tur blev ikke fundet." };
            }

            var acceptedRequests = trip.Requests.Count(r => r.RequestStatus == true);
            if (acceptedRequests >= trip.TripSeats)
            {
                return new OperationResult { Success = false, Message = "Kunne ikke acceptere anmodningen: Alle pladser er fuldt." };
            }

            var request = await _context.Request.FindAsync(requestId);
            if (request == null)
            {
                return new OperationResult { Success = false, Message = "Anmodning blev ikke fundet." };
            }

            request.RequestStatus = true;

            var driver = trip.Car?.Driver;
            if (driver != null)
            {
                driver.NumberOfPassengers += 1;
            }

            var user = await _context.User.FindAsync(request.UserId);
            if (user != null)
            {
                user.NumberOfTrips += 1;
            }

            await _context.SaveChangesAsync();

            return new OperationResult { Success = true, Message = "Anmodningen blev accepteret." };
        }



        public async Task<IEnumerable<Request>> GetRequestsForTripAsync(int tripId)
        {
            if (tripId <= 0)
            {
                throw new ArgumentException("Invalid trip ID", nameof(tripId));
            }

            return await _context.Request.Where(r => r.TripId == tripId).OrderBy(r => r.RequestTime).ToListAsync();
        }

        public async Task<OperationResult> RejectRequestAsync(int requestId)
        {
            var request = await _context.Request.FindAsync(requestId);
            if (request == null)
            {
                return new OperationResult { Success = false, Message = "Anmodningen blev ikke fundet." };
            }

            request.RequestStatus = false;
            await _context.SaveChangesAsync();
            return new OperationResult { Success = true, Message = "Anmodningen blev afvist." };
        }

        public async Task<bool> RequestExistsAsync(int tripId, int userId)
        {
            return await _context.Request.AnyAsync(r => r.TripId == tripId && r.UserId == userId);
        }

        public async Task<List<Request>> GetRequestsWithDetailsAsync(string? searchTerm = null)
        {
            var query = _context.Request.Include(r => r.Users).Include(r => r.Trip).AsQueryable();
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var lower = searchTerm.ToLower();
                query = query.Where(r => (r.RequestMessage != null && r.RequestMessage.ToLower().Contains(lower)) ||
                (r.RequestAddress != null && r.RequestAddress.ToLower().Contains(lower)) ||
                r.RequestTime.ToString().Contains(lower) ||
                ((lower == "accepteret" && r.RequestStatus == true) ||
                (lower == "afslået" && r.RequestStatus == false) ||
                (lower == "afventer" && r.RequestStatus == null)) ||
                (r.Users != null && (r.Users.FirstName.ToLower().Contains(lower) ||
                r.Users.LastName.ToLower().Contains(lower) ||
                r.Users.Email.ToLower().Contains(lower))) ||
                (r.Trip != null && (r.Trip.FromDestination.ToLower().Contains(lower) ||
                r.Trip.ToDestination.ToLower().Contains(lower))));
            }

            return await query.ToListAsync();
        }
    }
}
