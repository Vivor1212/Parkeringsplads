using Parkeringsplads.Models;

namespace Parkeringsplads.Services.Interfaces
{
    public interface IUser
    {
        Task<bool> CreateUserAsync(User user, string addressRoad, string addressNumber, int cityId);

    }
}
