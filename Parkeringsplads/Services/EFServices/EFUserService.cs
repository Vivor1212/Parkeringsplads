using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
    public async Task<bool> UpdateUserAsync(User updatedUser)
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

    public async Task<UserValidation> ValidateUserAsync(HttpContext httpContext)
    {
        var userEmail = httpContext.Session.GetString("UserEmail");
        if (string.IsNullOrEmpty(userEmail))
        {
            return new UserValidation
            {
                IsValid = false,
                ErrorMessage = "Please log in to continue.",
                RedirectPage = "/Account/Login/Login"
            };
        }

        var user = await _context.User
            .Include(u => u.School)
                .ThenInclude(s => s.Address)
                .ThenInclude(a => a.City)
            .FirstOrDefaultAsync(u => u.Email == userEmail);

        if (user == null)
        {
            return new UserValidation
            {
                IsValid = false,
                ErrorMessage = "User not found.",
                RedirectPage = "/Account/Login/Login"
            };
        }

        return new UserValidation
        {
            IsValid = true,
            User = user,
            ErrorMessage = null,
            RedirectPage = null
        };
    }

    public async Task<User?> GetUserWithDetailsByEmailAsync(string email)
    {
        return await _context.User.Include(u => u.School).ThenInclude(s => s.Address).ThenInclude(a => a.City).Include(u => u.UserAddresses).ThenInclude(ua => ua.Address).ThenInclude(a => a.City).FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetUserWithDetailsByIdAsync(int userId)
    {
        return await _context.User.Include(u => u.School).Include(u => u.UserAddresses).ThenInclude(ua => ua.Address).ThenInclude(a => a.City).FirstOrDefaultAsync(u => u.UserId == userId);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.User.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<bool> ChangePasswordAsync(string email, string currentPassword, string newPassword)
    {
        var user = await _context.User.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(currentPassword, user.Password))
        {
            return false;
        }

        user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ResetPasswordAsync(string email, string newPassword)
    {
        var user = await _context.User.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
        {
            return false;
        }

        user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<SelectListItem>> UserDropDownAsync()
    {
        return await _context.User.Select(u => new SelectListItem
        {
            Value = u.UserId.ToString(),
            Text = $"{u.FirstName} {u.LastName} ({u.Email})"
        }).ToListAsync();
    }

    public async Task<List<User>> GetUsersWithSchoolAsync(string? searchTerm = null)
    {
        var query = _context.User.Include(u => u.School).AsQueryable();
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var lowerTerm = searchTerm.ToLower();
            query = query.Where(u => u.FirstName.ToLower().Contains(lowerTerm) ||
            u.LastName.ToLower().Contains(lowerTerm) ||
            u.Email.ToLower().Contains(lowerTerm) ||
            u.Phone.ToLower().Contains(lowerTerm) ||
            u.Title.ToLower().Contains(lowerTerm) ||
            u.SchoolId.ToString().Contains(lowerTerm) ||
            (u.School != null && u.School.SchoolName.ToLower().Contains(lowerTerm)));
        }

        return await query.ToListAsync();
    }

    public async Task<User?> GetUserWithAddressesAsync(int userId)
    {
        return await _context.User.Include(u => u.UserAddresses).ThenInclude(ua => ua.Address).ThenInclude(a => a.City).FirstOrDefaultAsync(u => u.UserId == userId);
    }

    public async Task<List<SelectListItem>> GetNonDriverUsersAsync()
    {
        return await _context.User.Where(u => !_context.Driver.Any(d => d.UserId == u.UserId)).Select(u => new SelectListItem
        {
            Value = u.UserId.ToString(),
            Text = $"{u.FirstName} {u.LastName}"
        }).ToListAsync();
    }
}