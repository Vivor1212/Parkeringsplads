using Parkeringsplads.Models;
namespace Parkeringsplads.Services.Interfaces

{
    public interface ICarService
    {
        Task<List<Car>> GetAllCarsAsync();
        Task<List<Car>> GetCarsByDriverIdAsync(int driverId);
        Task<List<Car>> GetCarsAsync(string? searchTerm = null);
        Task<Car> GetCarByIdAsync(int carId);
        Task AddCarToDriverAsync(int driverId, Car car);
        Task UpdateCarAsync(Car car);
        Task DeleteCarAsync(int carId);
    }
}
