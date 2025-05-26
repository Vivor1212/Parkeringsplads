using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Parkeringsplads.Models;

namespace Parkeringsplads.Services.Interfaces
{

    public interface IDriverService
    {
        Task<Driver> GetDriverByUserIdAsync(int userId);
        
        Task<Driver> CreateDriverAsync(Driver driver);
        Task<List<SelectListItem>> GetAllDriversAsync();
        Task DeleteDriverAsync(int driverId);
    }
}
