using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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
    public async Task<bool> UpdateUserAsync(User updatedUser, string addressRoad, string addressNumber, int cityId)
    {
        var existingUser = await _context.User
            .Include(u => u.UserAddresses)
            .ThenInclude(ua => ua.Address)
            .FirstOrDefaultAsync(u => u.UserId == updatedUser.UserId);

        if (existingUser == null)
            return false;

        // Check for duplicate email (exclude current user)
        var existingEmailUser = await _context.User
            .FirstOrDefaultAsync(u => u.Email == updatedUser.Email && u.UserId != updatedUser.UserId);

        if (existingEmailUser != null)
            return false;

        // Update fields
        existingUser.FirstName = updatedUser.FirstName;
        existingUser.LastName = updatedUser.LastName;
        existingUser.Email = updatedUser.Email;
        existingUser.Phone = updatedUser.Phone;
        existingUser.Title = updatedUser.Title;
        existingUser.SchoolId = updatedUser.SchoolId;

        // --- Address handling ---

        // Find or create the address
        var existingAddress = await _context.Address
            .FirstOrDefaultAsync(a => a.AddressRoad == addressRoad && a.AddressNumber == addressNumber && a.CityId == cityId);

        int newAddressId;

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
            newAddressId = newAddress.AddressId;
        }
        else
        {
            newAddressId = existingAddress.AddressId;
        }

        // Get current UserAddress
        var userAddress = await _context.UserAddress
            .FirstOrDefaultAsync(ua => ua.User_Id == existingUser.UserId);

        if (userAddress != null && userAddress.Address_Id != newAddressId)
        {
            // Remove old link and create new one
            _context.UserAddress.Remove(userAddress);
            await _context.SaveChangesAsync();

            var newUserAddress = new UserAddress
            {
                User_Id = existingUser.UserId,
                Address_Id = newAddressId
            };
            _context.UserAddress.Add(newUserAddress);
        }

        await _context.SaveChangesAsync();

        return true;
    }


    public async Task<bool> DeleteUserAsync(int userId)
    {
        var user = await _context.User.FindAsync(userId);
        if (user == null)
        {
            return false; // User not found
        }

        // Find all requests linked to this user
        var requests = await _context.Request
            .Where(r => r.UserId == userId)
            .ToListAsync();

        // Delete the requests
        _context.Request.RemoveRange(requests);

        _context.User.Remove(user);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<User> GetUserAsync(int userId)
    {
        return await _context.User

          .FirstOrDefaultAsync(u => u.UserId == userId);

    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _context.User
               .Include(u => u.School)
        .Include(u => u.UserAddresses)
            .ThenInclude(ua => ua.Address)
                .ThenInclude(a => a.City)
                     .ToListAsync();
    }

    public async Task<UserValidation> ValidateDriverAsync(HttpContext httpContext)
    {
        var userEmail = httpContext.Session.GetString("UserEmail");
        if (string.IsNullOrEmpty(userEmail))
        {
            return new UserValidation
            {
                IsValid = false,
                RedirectPage = "./Login/Login",
                ErrorMessage = "Bruger er ikke logget ind."
            };
        }

        var user = await _context.User.FirstOrDefaultAsync(u => u.Email == userEmail);
        if (user == null)
        {
            return new UserValidation
            {
                IsValid = false,
                RedirectPage = "./Login/Login",
                ErrorMessage = "Bruger ikke fundet."
            };
        }

        var isDriver = await _context.Driver.AnyAsync(d => d.UserId == user.UserId);
        if (!isDriver)
        {
            return new UserValidation
            {
                IsValid = false,
                RedirectPage = "./Account/Profile",
                ErrorMessage = "Du skal være en chauffør for at udføre denne handling."
            };
        }

        return new UserValidation
        {
            IsValid = true,
            User = user
        };
    }

    public async Task<IEnumerable<Car>> GetDriverCarsAsync(int driverId)
    {
        return await _context.Car.Where(c => c.DriverId == driverId).ToListAsync();
    }
}