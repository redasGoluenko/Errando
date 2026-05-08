using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Errando.Data;
using Errando.DTOs;
using backend.Tests.Helpers;
using backend.Controllers;

namespace backend.Tests.Integration.Controllers
{
    public class ChatsControllerTests : IAsyncLifetime
    {
        private AppDbContext _context = null!;
        private ChatsController _controller = null!;

        public async Task InitializeAsync()
        {
            _context = TestSetup.CreateInMemoryContext();
            _controller = new ChatsController(_context);
            await TestSetup.SeedTestDataAsync(_context);
        }

        public async Task DisposeAsync()
        {
            await _context.DisposeAsync();
        }

        [Fact]
        [Trait("Category", "Integration")]
        [Trait("Feature", "Chats")]
        public async Task GetChats_ReturnsUsersChats()
        {
            // Arrange
            var user1 = await _context.Users.FirstAsync(u => u.Role == "Client");
            var user2 = await _context.Users.FirstAsync(u => u.Role == "Runner");

            var chat = new Chat
            {
                User1Id = user1.Id,
                User2Id = user2.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Chats.Add(chat);
            await _context.SaveChangesAsync();

            var claims = TestSetup.CreateClaimsPrincipal(user1.Id, user1.Username, "Client");
            _controller.ControllerContext.HttpContext = TestSetup.CreateMockHttpContext(claims);

            // Act
            var result = await _controller.GetChats();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var chats = Assert.IsAssignableFrom<IEnumerable<ChatDto>>(okResult.Value);
            Assert.NotEmpty(chats);
        }

        [Fact]
        [Trait("Category", "Integration")]
        [Trait("Feature", "Chats")]
        public async Task GetChats_WithNoChats_ReturnsEmptyList()
        {
            // Arrange
            var user = await _context.Users.FirstAsync(u => u.Role == "Admin");
            var claims = TestSetup.CreateClaimsPrincipal(user.Id, user.Username, "Admin");
            _controller.ControllerContext.HttpContext = TestSetup.CreateMockHttpContext(claims);

            // Act
            var result = await _controller.GetChats();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var chats = Assert.IsAssignableFrom<IEnumerable<ChatDto>>(okResult.Value);
            Assert.Empty(chats);
        }

        [Fact]
        [Trait("Category", "Integration")]
        [Trait("Feature", "Chats")]
        public async Task GetChats_ShowsOnlyUsersChats()
        {
            // Arrange
            var user1 = await _context.Users.FirstAsync(u => u.Role == "Client");
            var user2 = await _context.Users.FirstAsync(u => u.Role == "Runner");
            var admin = await _context.Users.FirstAsync(u => u.Role == "Admin");

            // Create chat between user1 and user2
            var chat1 = new Chat { User1Id = user1.Id, User2Id = user2.Id, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            // Create chat between admin and user2 (user1 should not see this)
            var chat2 = new Chat { User1Id = admin.Id, User2Id = user2.Id, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
            
            _context.Chats.AddRange(chat1, chat2);
            await _context.SaveChangesAsync();

            var claims = TestSetup.CreateClaimsPrincipal(user1.Id, user1.Username, "Client");
            _controller.ControllerContext.HttpContext = TestSetup.CreateMockHttpContext(claims);

            // Act
            var result = await _controller.GetChats();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var chats = Assert.IsAssignableFrom<IEnumerable<ChatDto>>(okResult.Value).ToList();
            Assert.Single(chats); // Should only see one chat
        }
    }
}
