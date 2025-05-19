using Microsoft.EntityFrameworkCore;
using Parkeringsplads.Models;
using Parkeringsplads.Services.Interfaces;

namespace Parkeringsplads.Services.EFServices
{
    public class EFAddressService : IAddressService
    {
        private readonly ParkeringspladsContext _context;

        public EFAddressService(ParkeringspladsContext context)
        {
            _context = context;
        }

        public async Task<Address> GetOrCreateAddressAsync(string road, string number, int cityId)
        {
            var existing = await _context.Address.FirstOrDefaultAsync(a =>
                a.AddressRoad == road && a.AddressNumber == number && a.CityId == cityId);

            if (existing != null)
                return existing;

            var newAddress = new Address
            {
                AddressRoad = road,
                AddressNumber = number,
                CityId = cityId
            };

            _context.Address.Add(newAddress);
            await _context.SaveChangesAsync();

            return newAddress;
        }

        public async Task<bool> LinkAddressToUserAsync(int userId, int addressId)
        {
            var exists = await _context.UserAddress.AnyAsync(ua =>
                ua.User_Id == userId && ua.Address_Id == addressId);

            if (exists) return false;

            _context.UserAddress.Add(new UserAddress
            {
                User_Id = userId,
                Address_Id = addressId
            });

            await _context.SaveChangesAsync();
            return true;
        }
    }

}
