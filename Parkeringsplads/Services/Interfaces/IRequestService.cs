using Parkeringsplads.Models;

namespace Parkeringsplads.Services.Interfaces
{
    public interface IRequestService
    {

        Task<List<Request>> GetAllRequestsForUser(User user);

       
    }
}
