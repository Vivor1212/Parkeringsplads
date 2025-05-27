using Parkeringsplads.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Parkeringsplads.Services.Interfaces
{
    public interface ITripService
    {
        Task AdminDeleteTripAsync(int tripId);
        Task<Trip> CreateTripAsync(Trip trip);
        Task<Trip?> GetTripByIdAsync(int tripId);
        Task<List<Trip>> GetAllAvailableTripsAsync(string? directionFilter, DateTime? dateFilter, int? hourFilter, string? cityFilter, string schoolAddress);
        Task<List<Trip>> GetAllTripsOnUserAsync(User user);
        Task<List<Trip>> GetTripsWithDetailsAsync();
        Task<List<Trip>> GetTripsWithDriverAsync(string? searchTerm = null);
        Task<OperationResult> DeleteTripAsync(int tripId, int userId);
        Task<TripValidation> GetDriverTripAsync(int tripId, int userId);
        Task<IEnumerable<Trip>> GetDriversFutureTripsAsync(int userId);
        Task<IEnumerable<Car>> GetDriversCarsAsync(int driverId);
        Task<Driver?> GetDriverWithCarsByEmailAsync(string email);
        Task<List<Trip>> GetAllTripsForDriverAsync(int userId);
        public string ExtractCity(string? address);

    }
}
