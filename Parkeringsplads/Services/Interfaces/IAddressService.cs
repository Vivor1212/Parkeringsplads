using Parkeringsplads.Models;

namespace Parkeringsplads.Services.Interfaces
{
    public interface IAddressService
    {
        Task<Address> GetOrCreateAddressAsync(string road, string number, int cityId);
        Task<Address> CreateAddressAsync(Address address);
        Task<bool> LinkAddressToUserAsync(int userId, int addressId);
        Task<bool> DeleteAddressAsync(int addressId);
        Task<List<Address>> GetUserAddressesAsync(int userId);
        Task<List<Address>> GetAddressesWithCityAsync(string? searchTerm = null);
    }

}
