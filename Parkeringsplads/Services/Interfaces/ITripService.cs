using Parkeringsplads.Models;
using System.Threading.Tasks;

namespace Parkeringsplads.Services.Interfaces
{
    public interface ITripService
    {
        Task CreateTripAsync(Trip trip);
    }
}
