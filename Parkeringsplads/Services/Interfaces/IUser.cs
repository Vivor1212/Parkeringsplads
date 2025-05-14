using Microsoft.EntityFrameworkCore;
using Parkeringsplads.Models;

namespace Parkeringsplads.Services.Interfaces
{
    public interface IUser
    {
        Task<bool> CreateUserAsync(User user, string addressRoad, string addressNumber, int cityId);

        Task<bool> UpdateUserAsync(User updatedUser);

        Task<bool> DeleteUserAsync(int userId);

        Task<User> GetUserAsync(int userId);

        Task<List<User>> GetAllUsersAsync();

    }
}
