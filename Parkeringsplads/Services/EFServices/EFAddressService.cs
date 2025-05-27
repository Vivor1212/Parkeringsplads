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
            var address = await _context.Address
                .Include(a => a.UserAddresses)
                .Include(a => a.Schools)
                .FirstOrDefaultAsync(a => a.AddressId == addressId);

            if (address == null)
            {
                return false; // Address not found
            }

            bool isInUse = address.UserAddresses.Any() || address.Schools.Any();
            if (isInUse)
            {
                return false; // Cannot delete - still in use
            }

            _context.Address.Remove(address);
            await _context.SaveChangesAsync();

            return true;
        }


        public async Task<List<Address>> GetUserAddressesAsync(int userId)
        {
            return await _context.UserAddress.Where(ua => ua.User_Id == userId).Include(ua => ua.Address).ThenInclude(a => a.City).Select(ua => ua.Address).ToListAsync();
        }

        public async Task<Address> CreateAddressAsync(Address address)
        {
            if (address == null)
                throw new ArgumentNullException(nameof(address), "Address cannot be null");

            _context.Address.Add(address);
            await _context.SaveChangesAsync();
            return address;
        }

        public async Task<List<Address>> GetAddressesWithCityAsync(string? searchTerm = null)
        {
            var query = _context.Address.Include(a => a.City).AsQueryable();
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var lower = searchTerm.ToLower();
                query = query.Where(a => a.AddressRoad.ToLower().Contains(lower) ||
                a.AddressNumber.ToLower().Contains(lower) ||
                a.City.CityName.ToLower().Contains(lower) ||
                a.City.PostalCode.Contains(lower));
            }

            return await query.ToListAsync();
        }

        public async Task<bool> IsAddressInUseAsync(int addressId)
        {
            return await _context.UserAddress.AnyAsync(ua => ua.Address_Id == addressId)
                || await _context.School.AnyAsync(s => s.AddressId == addressId);

        }
    }

}
