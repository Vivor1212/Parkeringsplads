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
            road = road?.Trim().ToLower();
            number = number?.Trim();

            var existing = await _context.Address.FirstOrDefaultAsync(a =>
                a.AddressRoad.ToLower() == road &&
                a.AddressNumber.Trim() == number &&
                a.CityId == cityId);

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

        public async Task<bool> DeleteAddressAsync(int addressId)
        {
            var address = await _context.Address.FindAsync(addressId);
            if (address == null)
            {
                return false; // Address not found
            }

            // Remove related records from UserAddress table
            var userAddresses = await _context.UserAddress
                .Where(ua => ua.Address_Id == addressId)
                .ToListAsync();

            _context.UserAddress.RemoveRange(userAddresses);

            // Delete the address
            _context.Address.Remove(address);
            await _context.SaveChangesAsync();

            return true;
        }
    }

}
