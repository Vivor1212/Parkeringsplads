using Parkeringsplads.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Parkeringsplads.Services.Interfaces
{
    public interface ITripService
    {
        Task<Trip> CreateTripAsync(Trip trip);

        Task<List<Trip>> GetAllAvailableTripsAsync(
            string? directionFilter,
            DateTime? dateFilter,
            int? hourFilter,
            string? cityFilter,
            string schoolAddress);

        Task<Trip?> GetTripByIdAsync(int tripId);

        Task AdminDeleteTripAsync(int tripId);

        Task<bool> DeleteTripAsync(int tripId, int userId);

    }
}
