using Parkeringsplads.Models;

namespace Parkeringsplads.Services.Interfaces
{
    public interface IAddressService
    {
        Task<Address> GetOrCreateAddressAsync(string road, string number, int cityId);
        Task<bool> LinkAddressToUserAsync(int userId, int addressId);
    }

}
