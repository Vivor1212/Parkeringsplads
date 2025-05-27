using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Parkeringsplads.Models;

namespace Parkeringsplads.Services.Interfaces
{

    public interface IDriverService
    {
        Task<Driver> GetDriverByUserIdAsync(int userId);
        
        Task<Driver> CreateDriverAsync(Driver driver);
        Task<List<Driver>> GetDriversWithUserAsync(string? searchTerm = null);
        Task<List<SelectListItem>> GetAllDriversAsync();
        Task DeleteDriverAsync(int driverId);
        Task<Driver?> GetDriverWithUserAsync(int driverId);
        Task<Driver?> GetDriverWithDetailsAsync(int driverId);
        Task<Driver?> GetDriverByUserIdAsync(int userId);
        Task<bool> UpdateDriverAsync(int driverId, string driverLicense, string driverCpr, int userId);
        Task<bool> UnbecomeDriver(int userId);
        Task<bool> DriverExistsAsync(int userId);
        Task<DriverValidation> ValidateAndCreateDriverAdync(Driver driver);
    }
}
