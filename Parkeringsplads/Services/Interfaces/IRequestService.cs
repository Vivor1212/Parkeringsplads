using Parkeringsplads.Models;

namespace Parkeringsplads.Services.Interfaces
{
    public interface IRequestService
    {

        Task DeleteRequestAsync(int requestId);
        Task<List<Request>> GetAllRequestsForUser(User user);
        Task<List<Request>> GetRequestsWithDetailsAsync(string? searchTerm = null);
        Task<Request> GetRequestByIdAsync(int requestId);
        Task<Request> CreateRequestAsync(Request request);
        Task<OperationResult> AcceptRequestAsync(int requestId, int tripId);
        Task<OperationResult> RejectRequestAsync(int requestId, int tripId);
        Task<IEnumerable<Request>> GetRequestsForTripAsync(int tripId);
        Task<bool> RequestExistsAsync(int tripId, int userId);
    }
}
