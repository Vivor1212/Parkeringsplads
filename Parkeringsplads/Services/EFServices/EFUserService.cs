using Microsoft.EntityFrameworkCore;
using Parkeringsplads.Models;
using Parkeringsplads.Services.Interfaces;
using System.Threading.Tasks;

public class EFUserService : IUser
{
    private readonly ParkeringspladsContext _context;

    public EFUserService(ParkeringspladsContext context)
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
    public async Task<bool> UpdateUserAsync(User updatedUser)
    {
        var existingUser = await _context.User.FindAsync(updatedUser.UserId);

        if (existingUser == null)
        {
            return false; // User not found
        }

        // Check if the new email is already in use by another user
        var existingEmailUser = await _context.User
            .FirstOrDefaultAsync(u => u.Email == updatedUser.Email && u.UserId != updatedUser.UserId);

        if (existingEmailUser != null)
        {
            return false;  // Email is already in use by someone else
        }

        if (!string.IsNullOrWhiteSpace(updatedUser.FirstName))
            existingUser.FirstName = updatedUser.FirstName;

        if (!string.IsNullOrWhiteSpace(updatedUser.LastName))
            existingUser.LastName = updatedUser.LastName;

        if (!string.IsNullOrWhiteSpace(updatedUser.Email))
            existingUser.Email = updatedUser.Email;

        if (!string.IsNullOrWhiteSpace(updatedUser.Phone))
            existingUser.Phone = updatedUser.Phone;

        if (!string.IsNullOrWhiteSpace(updatedUser.Title))
            existingUser.Title = updatedUser.Title;


        if (updatedUser.SchoolId.HasValue)
            existingUser.SchoolId = updatedUser.SchoolId.Value;
        // Save changes to the database
        await _context.SaveChangesAsync();

        return true;
    }

}
