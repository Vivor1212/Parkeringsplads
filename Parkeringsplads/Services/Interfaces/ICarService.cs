using Parkeringsplads.Models;
namespace Parkeringsplads.Services.Interfaces

{
    public interface ICarService
    {
        Task<List<Car>> GetAllCarsAsync();
        Task<Car> GetCarByIdAsync(int carId);
        Task<List<Car>> GetCarsByDriverIdAsync(int driverId);
        Task AddCarToDriverAsync(int driverId, Car car);
        Task UpdateCarAsync(Car car);
        Task DeleteCarAsync(int carId);
    }
}
