using KickFive.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace KickFive.Data
{
    public class DbSeeder
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;

        public DbSeeder(UserManager<User> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task SeedAsync(KickFiveContext context)
        {
            if (!context.Field.Any())
            {
                var fields = new List<Field>
                {
                    new Field {Name="Field1"},
                    new Field {Name="Field2"},
                    new Field {Name="Field3"},
                    new Field {Name="Field4"}
                };
                context.Field.AddRange(fields);
                await context.SaveChangesAsync();
            }

            // Seed regular users only if they don't already exist
            var seedUsers = new List<(string FirstName, string LastName, string Email)>
            {
                ("John", "Doe", "johndoe@gmail.com"),
                ("User1", "User1", "user1@gmail.com"),
                ("User2", "User2", "user2@gmail.com"),
                ("Jane", "Smith", "janesmith@gmail.com"),
                ("User3", "User3", "user3@gmail.com"),
            };

            foreach (var (firstName, lastName, email) in seedUsers)
            {
                var existingUser = await _userManager.FindByEmailAsync(email);
                if (existingUser == null)
                {
                    var user = new User
                    {
                        UserName = email,
                        Email = email,
                        FirstName = firstName,
                        LastName = lastName,
                        PhoneNumber = "02909402",
                        EmailConfirmed = true
                    };

                    var result = await _userManager.CreateAsync(user, "Password123!");
                    if (!result.Succeeded)
                    {
                        foreach (var error in result.Errors)
                            Console.WriteLine($"Seed user error ({email}): {error.Code} - {error.Description}");
                    }
                }
            }

            if (!context.Booking.Any())
            {
                var adminEmail = _configuration["AdminUser:Email"];

                var users = await context.User
                    .Where(u => u.Email != adminEmail)
                    .OrderBy(u => u.Email)
                    .ToListAsync();

                var fields = await context.Field.ToListAsync();

                if (users.Count >= 4 && fields.Count >= 1)
                {
                    var fieldId = fields[0].Id; // use whatever the real Id is

                    var bookings = new List<Booking>
                    {
                        new Booking { StartDateTime = new DateTime(2026, 8, 22, 7, 0, 0), EndDateTime = new DateTime(2026, 8, 22, 8, 0, 0), Status = "Cancelled", Price = 80.00m, FieldId = fieldId, UserId = users[0].Id },
                        new Booking { StartDateTime = new DateTime(2026, 8, 22, 6, 0, 0), EndDateTime = new DateTime(2026, 8, 22, 7, 0, 0), Status = "Pending",   Price = 80.00m, FieldId = fieldId, UserId = users[1].Id },
                        new Booking { StartDateTime = new DateTime(2026, 8, 22, 9, 0, 0), EndDateTime = new DateTime(2026, 8, 22, 10, 0, 0), Status = "Cancelled", Price = 80.00m, FieldId = fieldId, UserId = users[2].Id },
                        new Booking { StartDateTime = new DateTime(2026, 8, 22, 10, 0, 0), EndDateTime = new DateTime(2026, 8, 22, 11, 0, 0), Status = "Cancelled", Price = 80.00m, FieldId = fieldId, UserId = users[3].Id },
                    };
                    context.Booking.AddRange(bookings);
                    await context.SaveChangesAsync();
                }
                else
                {
                    Console.WriteLine($"Warning: Not enough data to seed bookings. Users: {users.Count}, Fields: {fields.Count}");
                }
            }
        }
    }
}