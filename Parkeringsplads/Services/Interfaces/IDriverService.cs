using Microsoft.AspNetCore.Mvc.Rendering;
using Parkeringsplads.Models;

namespace Parkeringsplads.Services.Interfaces
{
    public interface IDriverService
    {
        Task<Driver> CreateDriverAsync(Driver driver);
        Task<List<SelectListItem>> GetAllDriversAsync();
        Task DeleteDriverAsync(int driverId);
    }
}
