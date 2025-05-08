using Parkeringsplads.Models;

namespace Parkeringsplads.Services.Interfaces
{
    public interface IDriverService
    {
        Task<Driver> CreateDriverAsync(Driver driver);
    }
}
