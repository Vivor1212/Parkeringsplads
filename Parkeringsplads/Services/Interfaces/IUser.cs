using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Parkeringsplads.Models;

namespace Parkeringsplads.Services.Interfaces
{
    public interface IUser
    {
        Task<User> GetUserByEmailAsync(string email);
        Task<bool> CreateUserAsync(User user, string addressRoad, string addressNumber, int cityId);
        Task<bool> UpdateUserAsync(User updatedUser);
        Task<bool> DeleteUserAsync(int userId);
        Task<bool> ChangePasswordAsync(string email, string currentPassword, string newPassword);
        Task<bool> ResetPasswordAsync(string email, string newPassword);
        Task<User> GetUserAsync(int userId);
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserWithDetailsByEmailAsync(string email);
        Task<User?> GetUserWithDetailsByIdAsync(int userId);
        Task<User?> GetUserWithAddressesAsync(int userId);
        Task<List<User>> GetAllUsersAsync();
        Task<List<User>> GetUsersWithSchoolAsync(string? searchTerm = null);
        Task<List<SelectListItem>> UserDropDownAsync();
        Task<List<SelectListItem>> GetNonDriverUsersAsync();
        Task<UserValidation> ValidateUserAsync(HttpContext httpContext);
        Task<UserValidation> ValidateDriverAsync(HttpContext httpContext);
    }
}
