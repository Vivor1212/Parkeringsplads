using Parkeringsplads.Models;

namespace Parkeringsplads.Services.Interfaces
{
    public interface IRequestService
    {

        Task<List<Request>> GetAllRequestsForUser(User user);
        Task<Request> GetRequestByIdAsync(int requestId);
        Task DeleteRequestAsync(int requestId);
        Task<Request> CreateRequestAsync(Request request);
        Task<Request> AcceptRequestAsync(int requestId);
        Task<Request> RejectRequestAsync(int requestId);
        Task<IEnumerable<Request>> GetRequestsForTripAsync(int tripId);
    }
}
