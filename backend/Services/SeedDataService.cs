using BCrypt.Net;
using Errando.Data;
using Microsoft.EntityFrameworkCore;

namespace Errando.Services
{
    public static class SeedDataService
    {
        public static async Task SeedDefaultDataAsync(AppDbContext context)
        {
            try
            {
                // Create admin user if doesn't exist
                if (!await context.Users.AnyAsync(u => u.Username == "admin"))
                {
                    var adminUser = new User
                    {
                        Username = "admin",
                        Email = "admin@errando.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123"),
                        Role = "Admin"
                    };
                    context.Users.Add(adminUser);
                }

                // Create client user if doesn't exist
                if (!await context.Users.AnyAsync(u => u.Username == "client"))
                {
                    var clientUser = new User
                    {
                        Username = "client",
                        Email = "client@errando.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Client123"),
                        Role = "Client"
                    };
                    context.Users.Add(clientUser);
                }

                // Create runner user if doesn't exist
                if (!await context.Users.AnyAsync(u => u.Username == "runner"))
                {
                    var runnerUser = new User
                    {
                        Username = "runner",
                        Email = "runner@errando.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Runner123"),
                        Role = "Runner"
                    };
                    context.Users.Add(runnerUser);
                }

                await context.SaveChangesAsync();
                Console.WriteLine("✅ Default users ready: admin/Admin123, client/Client123, runner/Runner123");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Seeding error: {ex.Message}");
            }
        }
    }
}
