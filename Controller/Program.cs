using Microsoft.EntityFrameworkCore;
using Parkeringsplads.Models;

class Program
{
    static void Main()
    {
        #region Password convertion
        // Replace with your actual DB connection string
        var optionsBuilder = new DbContextOptionsBuilder<ParkeringspladsContext>();
        optionsBuilder.UseSqlServer("");

        using var context = new ParkeringspladsContext(optionsBuilder.Options);

        // Replace this with the email of the user you want to hash
        string targetEmail = "Emil@mail.dk";

        var user = context.User.FirstOrDefault(u => u.Email == targetEmail);

        if (user == null)
        {
            Console.WriteLine($"❌ User with email '{targetEmail}' not found.");
            return;
        }

        if (IsHashed(user.Password))
        {
            Console.WriteLine($"⚠️ Password for '{user.Email}' is already hashed. No action taken.");
            return;
        }

        string plainPassword = user.Password;
        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(plainPassword);

        user.Password = hashedPassword;
        context.SaveChanges();

        Console.WriteLine($"✅ Password for '{user.Email}' has been securely hashed and saved.");
        #endregion 
    }

    static bool IsHashed(string password)
    {
        return password.StartsWith("$2a$") || password.StartsWith("$2b$") || password.StartsWith("$2y$");
    }
}