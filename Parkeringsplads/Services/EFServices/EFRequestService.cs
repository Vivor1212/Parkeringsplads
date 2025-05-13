using Microsoft.EntityFrameworkCore;
using Parkeringsplads.Models;
using Parkeringsplads.Services.Interfaces;

namespace Parkeringsplads.Services.EFServices
{
    public class EFRequestService : IRequestService
    {
        private ParkeringspladsContext _context;

        public EFRequestService(ParkeringspladsContext service)
        {
            _context = service;
        }


        public async Task<Request> CreateRequestAsync(Request request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "Request cannot be null");
            }
            await _context.Request.AddAsync(request);
            return request;
        }


        public async Task<Request> AcceptRequestAsync(int requestId)
        {
            if (requestId <= 0)
            {
                throw new ArgumentException("Invalid request ID", nameof(requestId));
            }

            var request = await _context.Request.Include(r => r.Trip).FirstOrDefaultAsync(r => r.RequestId == requestId);

            if (request == null)
            {
                throw new ArgumentException($"Request with ID {requestId} not found");
            }
            if (request.RequestStatus != null)
            {
                throw new InvalidOperationException("Request is not in pending status");
            }
            if (request.Trip.TripSeats <= 0)
            {
                throw new InvalidOperationException("No available seats for this trip");
            }

            request.RequestStatus = true;
            request.Trip.TripSeats--;

            await _context.SaveChangesAsync();
            return request;
        }

        public async Task<IEnumerable<Request>> GetRequestsForTripAsync(int tripId)
        {
            if (tripId <= 0)
            {
                throw new ArgumentException("Invalid trip ID", nameof(tripId));
            }

            return await _context.Request.Where(r => r.TripId == tripId).OrderBy(r => r.RequestTime).ToListAsync();
        }

        public async Task<Request> RejectRequestAsync(int requestId)
        {
            if (requestId <= 0)
            {
                throw new ArgumentException("Invalid request ID", nameof(requestId));
            }

            var request = await _context.Request.FirstOrDefaultAsync(r => r.RequestId == requestId);

            if (request == null)
            {
                throw new ArgumentException($"Request with ID {requestId} not found");
            }
            if (request.RequestStatus != null)
            {
                throw new InvalidOperationException("Request is not in pending status");
            }

            request.RequestStatus = false;
            await _context.SaveChangesAsync();
            return request;
        }
    }
}
