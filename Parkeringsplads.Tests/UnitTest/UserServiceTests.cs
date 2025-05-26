using Microsoft.EntityFrameworkCore;
using Parkeringsplads.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parkeringsplads.Tests.UnitTest
{
    public class UserServiceTests
    {
        private ParkeringspladsContext GetInMemoryDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ParkeringspladsContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            return new ParkeringspladsContext(options);
        }

        [Fact]
        public async Task CreateUserAsync_ShouldCreateUser_WhenValidInput()
        {
            var context = GetInMemoryDbContext("TestDb1");
            var service = new EFUserService(context);
            var city = new City
            {
                CityId = 1,
                CityName = "Roskilde",
                PostalCode = "4000"
            };

            context.City.Add(city);
            await context.SaveChangesAsync();



            var user = new User
            {
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "Bruger",
                Password = "password123",
                Phone = "12345678",
                Title = "Studerende"
            };

            var result = await service.CreateUserAsync(user, "Testvej", "10", 1);
            // Log brugere
            foreach (var u in context.User)
            {
                Console.WriteLine($"User: Id={u.UserId}, Email={u.Email}, Name={u.FirstName} {u.LastName}");
            }

            // Log adresser
            foreach (var a in context.Address)
            {
                Console.WriteLine($"Address: Id={a.AddressId}, Road={a.AddressRoad} {a.AddressNumber}, CityId={a.CityId}");
            }

            // Log UserAddress koblinger
            foreach (var ua in context.UserAddress)
            {
                Console.WriteLine($"UserAddress: UserId={ua.User_Id}, AddressId={ua.Address_Id}");
            }

            Assert.True(result);  // bekræft at brugeren blev oprettet
            Assert.Single(context.User); // tjek der kun er én bruger
            Assert.Single(context.Address); // tjek én adresse er oprettet
            Assert.Single(context.UserAddress); // tjek kobling findes
        }
    }
}
