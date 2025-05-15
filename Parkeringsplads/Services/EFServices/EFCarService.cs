using Microsoft.EntityFrameworkCore;
using Parkeringsplads.Models;
using Parkeringsplads.Services.Interfaces;
namespace Parkeringsplads.Services.EFServices
{
    public class EFCarService : ICarService
    {
        private readonly ParkeringspladsContext _context;

        public EFCarService(ParkeringspladsContext context)
        {
            _context = context;
        }

        public IList<Car> Car { get; set; }

        public async Task<List<Car>> GetAllCarsAsync()
        {
            return await _context.Car
        .Include(c => c.Driver) 
        .ToListAsync();
        }

        public async Task<Car> GetCarByIdAsync(int carId)
        {
            return await _context.Car
                .Include(c => c.Driver)
                .FirstOrDefaultAsync(c => c.CarId == carId);
        }

        public async Task<List<Car>> GetCarsByDriverIdAsync(int driverId)
        {
            return await _context.Car
                .Where(c => c.DriverId == driverId)
                .ToListAsync();
        }
        public async Task AddCarToDriverAsync(int driverId, Car car)
        {
            var driver = await _context.Driver
                .Include(d => d.Cars)
                .FirstOrDefaultAsync(d => d.DriverId == driverId);

            if (driver != null)
            {
                car.DriverId = driverId;

                driver.Cars.Add(car);

                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateCarAsync(Car updatedCar)
        {
            var existingCar = await _context.Car.FirstOrDefaultAsync(c => c.CarId == updatedCar.CarId);
            if (existingCar != null)
            {
                existingCar.CarModel = updatedCar.CarModel;
                existingCar.CarPlate = updatedCar.CarPlate;
                existingCar.CarCapacity = updatedCar.CarCapacity;

                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteCarAsync(int carId)
        {
            var car = await _context.Car.FirstOrDefaultAsync(c => c.CarId == carId);
            if (car != null)
            {
                _context.Car.Remove(car);
                await _context.SaveChangesAsync();
            }
        }
    }
}
