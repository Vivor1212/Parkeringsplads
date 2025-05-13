using Parkeringsplads.Models;

namespace Parkeringsplads.Services.Interfaces
{
    public interface IRequestService
    {

        Task<List<Request>> GetAllRequestsForUser(User user);
        Task<Request> GetRequestByIdAsync(int requestId);


        Task DeleteRequestAsync(int requestId);


    }
}
