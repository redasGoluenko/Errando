using Microsoft.EntityFrameworkCore;
using Errando.Data;
using Errando.Services;
using Moq;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace backend.Tests.Helpers
{
    /// <summary>
    /// Helper class for setting up test dependencies and data.
    /// </summary>
    public static class TestSetup
    {
        /// <summary>
        /// Creates a fresh in-memory database context for each test.
        /// </summary>
        public static AppDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new AppDbContext(options);
        }

        /// <summary>
        /// Seeds basic test data into the context.
        /// </summary>
        public static async Task SeedTestDataAsync(AppDbContext context)
        {
            var admin = new User
            {
                Id = 1,
                Username = "admin",
                Email = "admin@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123"),
                Role = "Admin"
            };

            var client = new User
            {
                Id = 2,
                Username = "client",
                Email = "client@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Client123"),
                Role = "Client"
            };

            var runner = new User
            {
                Id = 3,
                Username = "runner",
                Email = "runner@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Runner123"),
                Role = "Runner"
            };

            context.Users.AddRange(admin, client, runner);
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Creates mock IConfiguration for testing.
        /// </summary>
        public static Mock<IConfiguration> CreateMockConfig()
        {
            var config = new Mock<IConfiguration>();
            config.Setup(x => x["Jwt:Key"]).Returns("test_secret_key_that_is_long_enough_for_jwt_validation");
            return config;
        }

        /// <summary>
        /// Creates mock IEmailService for testing.
        /// </summary>
        public static Mock<IEmailService> CreateMockEmailService()
        {
            var emailService = new Mock<IEmailService>();
            emailService
                .Setup(x => x.SendRegistrationConfirmationAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);
            return emailService;
        }

        /// <summary>
        /// Creates a ClaimsPrincipal for a specific role.
        /// </summary>
        public static ClaimsPrincipal CreateClaimsPrincipal(int userId, string username, string role)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role)
            };

            var identity = new ClaimsIdentity(claims, "test");
            return new ClaimsPrincipal(identity);
        }

        /// <summary>
        /// Creates a mock HttpContext with a specific user.
        /// </summary>
        public static HttpContext CreateMockHttpContext(ClaimsPrincipal user)
        {
            var context = new Mock<HttpContext>();
            context.Setup(x => x.User).Returns(user);
            return context.Object;
        }
    }
}
