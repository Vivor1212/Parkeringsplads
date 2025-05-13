using Parkeringsplads.Models;

namespace Parkeringsplads.Services.Interfaces
{
    public interface IRequestService
    {
        Task<Request> CreateRequestAsync(Request request);
        Task<Request> AcceptRequestAsync(int requestId);
        Task<Request> RejectRequestAsync(int requestId);
        Task<IEnumerable<Request>> GetRequestsForTripAsync(int tripId);
    }
}
