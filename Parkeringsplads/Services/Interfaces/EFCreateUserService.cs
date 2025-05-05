using Microsoft.EntityFrameworkCore;
using Parkeringsplads.Models;
using Parkeringsplads.Services.EFServices;
using System.Threading.Tasks;

public class EFCreateUserService : ICreateUser
{
    private readonly ParkeringspladsContext _context;

    public EFCreateUserService(ParkeringspladsContext context)
    {
        _context = context;
    }

    public async Task<bool> CreateUserAsync(User user, string addressRoad, string addressNumber, int cityId)
    {
        if (user == null) return false;

        // 1. Check if the address already exists
        var existingAddress = await _context.Address
            .FirstOrDefaultAsync(a => a.AddressRoad == addressRoad && a.AddressNumber == addressNumber && a.CityId == cityId);

        int addressId;

        // If the address doesn't exist, create a new one
        if (existingAddress == null)
        {
            var newAddress = new Address
            {
                AddressRoad = addressRoad,
                AddressNumber = addressNumber,
                CityId = cityId
            };
            _context.Address.Add(newAddress);
            await _context.SaveChangesAsync();
            addressId = newAddress.AddressId;
        }
        else
        {
            // If the address exists, use the existing one
            addressId = existingAddress.AddressId;
        }

        // 2. Check if the email already exists
        var existingUser = await _context.User.FirstOrDefaultAsync(u => u.Email == user.Email);
        if (existingUser != null)
        {
            return false;  // Email is already in use
        }

        //  ---- Hash the password before saving
       // var passwordHasher = new Microsoft.AspNetCore.Identity.PasswordHasher<User>();
         //user.Password = passwordHasher.HashPassword(user, user.Password); 

        // 4. Add the new user
        _context.User.Add(user);
        await _context.SaveChangesAsync();

        // 5. Create the UserAddress linking table entry
        var userAddress = new UserAddress
        {
            User_Id = user.UserId,
            Address_Id = addressId
        };
        _context.UserAddress.Add(userAddress);
        await _context.SaveChangesAsync();

        return true;
    }
}
