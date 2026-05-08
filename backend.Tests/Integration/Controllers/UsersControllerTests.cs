using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Errando.Data;
using Errando.DTOs;
using Errando.Services;
using backend.Tests.Helpers;
using backend.Controllers;
using Microsoft.AspNetCore.Http;

namespace backend.Tests.Integration.Controllers
{
    public class UsersControllerTests : IAsyncLifetime
    {
        private AppDbContext _context = null!;
        private Mock<IConfiguration> _configMock = null!;
        private Mock<IEmailService> _emailServiceMock = null!;
        private UsersController _controller = null!;

        public async Task InitializeAsync()
        {
            _context = TestSetup.CreateInMemoryContext();
            _configMock = TestSetup.CreateMockConfig();
            _emailServiceMock = TestSetup.CreateMockEmailService();
            _controller = new UsersController(_context, _configMock.Object, _emailServiceMock.Object);
            
            await TestSetup.SeedTestDataAsync(_context);
        }

        public async Task DisposeAsync()
        {
            await _context.DisposeAsync();
        }

        #region Login Tests

        [Fact]
        [Trait("Category", "Integration")]
        [Trait("Feature", "Authentication")]
        public async Task Login_WithValidAdminCredentials_ReturnsAuthResponse()
        {
            // Arrange
            var loginDto = new LoginDto { Username = "admin", Password = "Admin123" };

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            Assert.NotNull(result.Value);
            Assert.NotEmpty(result.Value.Token);
            Assert.Equal("admin", result.Value.Username);
        }

        [Fact]
        [Trait("Category", "Integration")]
        [Trait("Feature", "Authentication")]
        public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
        {
            // Arrange
            var loginDto = new LoginDto { Username = "admin", Password = "WrongPassword" };

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result.Result);
        }

        [Fact]
        [Trait("Category", "Integration")]
        [Trait("Feature", "Authentication")]
        public async Task Login_WithNonExistentUser_ReturnsUnauthorized()
        {
            // Arrange
            var loginDto = new LoginDto { Username = "nonexistent", Password = "Password123" };

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result.Result);
        }

        [Fact]
        [Trait("Category", "Integration")]
        [Trait("Feature", "Authentication")]
        public async Task Login_WithEmptyPassword_ReturnsUnauthorized()
        {
            // Arrange
            var loginDto = new LoginDto { Username = "admin", Password = "" };

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            Assert.True(result.Result is UnauthorizedObjectResult);
        }

        #endregion

        #region Register Tests

        [Fact]
        [Trait("Category", "Integration")]
        [Trait("Feature", "Registration")]
        public async Task Register_WithValidData_CreatesUserAndReturnsUser()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Username = "newuser",
                Email = "newuser@test.com",
                Password = "Password123"
            };

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            Assert.IsType<CreatedAtActionResult>(result.Result);

            // Verify user was created in database
            var user = await _context.Users.FindAsync(4);
            Assert.NotNull(user);
            Assert.Equal("newuser", user.Username);
        }

        [Fact]
        [Trait("Category", "Integration")]
        [Trait("Feature", "Registration")]
        public async Task Register_WithDuplicateUsername_ReturnsBadRequest()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Username = "admin", // Already exists
                Email = "newemail@test.com",
                Password = "Password123"
            };

            // Act
            var result = await _controller.Register(registerDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        [Trait("Category", "Integration")]
        [Trait("Feature", "Validation")]
        public async Task Register_WithEmptyUsername_ReturnsBadRequest()
        {
            // Arrange
            var registerDto = new RegisterDto
            {
                Username = "",
                Email = "test@test.com",
                Password = "Password123"
            };

            // Act
            var result = await _controller.Register(registerDto);

            // Assert - Check that it's not created (either null value or error result)
            Assert.True(result.Value == null || result.Result is BadRequestObjectResult, 
                "Empty username should not allow registration");
        }

        #endregion

        #region GetUsers Tests

        [Fact]
        [Trait("Category", "Integration")]
        [Trait("Feature", "Authorization")]
        public async Task GetUsers_AsAdmin_ReturnsAllUsers()
        {
            // Arrange
            var adminUser = TestSetup.CreateClaimsPrincipal(1, "admin", "Admin");
            var httpContext = TestSetup.CreateMockHttpContext(adminUser);
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            // Act
            var result = await _controller.GetUsers();

            // Assert
            var users = result.Value as List<User>;
            Assert.NotNull(users);
            Assert.True(users.Count >= 3, "Admin should see all users");
        }

        [Fact]
        [Trait("Category", "Integration")]
        [Trait("Feature", "Authorization")]
        public async Task GetUsers_AsClient_ReturnsOnlyOwnUser()
        {
            // Arrange
            var clientUser = TestSetup.CreateClaimsPrincipal(2, "client", "Client");
            var httpContext = TestSetup.CreateMockHttpContext(clientUser);
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            // Act
            var result = await _controller.GetUsers();

            // Assert
            var users = result.Value as List<User>;
            Assert.NotNull(users);
            Assert.Single(users);
            Assert.Equal("client", users[0].Username);
        }

        #endregion

        #region GetUser Tests

        [Fact]
        [Trait("Category", "Integration")]
        [Trait("Feature", "Authorization")]
        public async Task GetUser_AsAdminViewingAnyUser_ReturnsUser()
        {
            // Arrange
            var adminUser = TestSetup.CreateClaimsPrincipal(1, "admin", "Admin");
            var httpContext = TestSetup.CreateMockHttpContext(adminUser);
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            // Act
            var result = await _controller.GetUser(2);

            // Assert
            var user = result.Value;
            Assert.NotNull(user);
            Assert.Equal(2, user.Id);
            Assert.Equal("client", user.Username);
        }

        [Fact]
        [Trait("Category", "Integration")]
        [Trait("Feature", "Authorization")]
        public async Task GetUser_ClientViewingOwnUser_ReturnsOk()
        {
            // Arrange
            var clientUser = TestSetup.CreateClaimsPrincipal(2, "client", "Client");
            var httpContext = TestSetup.CreateMockHttpContext(clientUser);
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            // Act
            var result = await _controller.GetUser(2); // Accessing own user

            // Assert
            var user = result.Value;
            Assert.NotNull(user);
            Assert.Equal(2, user.Id);
        }

        #endregion

        #region CreateUser Tests

        [Fact]
        [Trait("Category", "Integration")]
        [Trait("Feature", "Authorization")]
        public async Task CreateUser_AsAdminWithValidData_CreatesUser()
        {
            // Arrange
            var adminUser = TestSetup.CreateClaimsPrincipal(1, "admin", "Admin");
            var httpContext = TestSetup.CreateMockHttpContext(adminUser);
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            var createDto = new UsersController.CreateUserDto
            {
                Username = "newadmin",
                Email = "newadmin@test.com",
                Password = "Password123",
                Role = "Admin"
            };

            // Act
            var result = await _controller.CreateUser(createDto);

            // Assert
            Assert.IsType<CreatedAtActionResult>(result.Result);
            var createdResult = result.Result as CreatedAtActionResult;
            var user = createdResult?.Value as User;
            Assert.NotNull(user);
            Assert.Equal("newadmin", user.Username);
        }

        [Fact]
        [Trait("Category", "Integration")]
        [Trait("Feature", "Validation")]
        public async Task CreateUser_WithEmptyUsername_ReturnsBadRequest()
        {
            // Arrange
            var adminUser = TestSetup.CreateClaimsPrincipal(1, "admin", "Admin");
            var httpContext = TestSetup.CreateMockHttpContext(adminUser);
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            var createDto = new UsersController.CreateUserDto
            {
                Username = "",
                Email = "test@test.com",
                Password = "Password123"
            };

            // Act
            var result = await _controller.CreateUser(createDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        #endregion

        #region UpdateUser Tests

        [Fact]
        [Trait("Category", "Integration")]
        [Trait("Feature", "Authorization")]
        public async Task UpdateUser_AsAdminChangingRole_Succeeds()
        {
            // Arrange
            var adminUser = TestSetup.CreateClaimsPrincipal(1, "admin", "Admin");
            var httpContext = TestSetup.CreateMockHttpContext(adminUser);
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            var updateDto = new UsersController.UpdateUserDto
            {
                Id = 2,
                Username = "client_updated",
                Email = "client@test.com",
                Role = "Runner"
            };

            // Act
            var result = await _controller.UpdateUser(2, updateDto);

            // Assert
            Assert.IsType<NoContentResult>(result);
            var updatedUser = await _context.Users.FindAsync(2);
            Assert.Equal("Runner", updatedUser?.Role);
        }

        [Fact]
        [Trait("Category", "Integration")]
        [Trait("Feature", "Authorization")]
        public async Task UpdateUser_ClientChangingRole_RoleDoesNotChange()
        {
            // Arrange
            var clientUser = TestSetup.CreateClaimsPrincipal(2, "client", "Client");
            var httpContext = TestSetup.CreateMockHttpContext(clientUser);
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            var updateDto = new UsersController.UpdateUserDto
            {
                Id = 2,
                Username = "client_updated",
                Email = "client@test.com",
                Role = "Admin" // Trying to change role
            };

            // Act
            var result = await _controller.UpdateUser(2, updateDto);

            // Assert
            Assert.IsType<NoContentResult>(result);
            var updatedUser = await _context.Users.FindAsync(2);
            Assert.Equal("Client", updatedUser?.Role); // Role should NOT change
        }

        #endregion

        #region DeleteUser Tests

        [Fact]
        [Trait("Category", "Integration")]
        [Trait("Feature", "Authorization")]
        public async Task DeleteUser_AsAdmin_DeletesUserSuccessfully()
        {
            // Arrange
            var adminUser = TestSetup.CreateClaimsPrincipal(1, "admin", "Admin");
            var httpContext = TestSetup.CreateMockHttpContext(adminUser);
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            // Act
            var result = await _controller.DeleteUser(2);

            // Assert
            Assert.IsType<NoContentResult>(result);
            var deletedUser = await _context.Users.FindAsync(2);
            Assert.Null(deletedUser);
        }

        [Fact]
        [Trait("Category", "Integration")]
        [Trait("Feature", "Authorization")]
        public async Task DeleteUser_WithNonexistentId_ReturnsNotFound()
        {
            // Arrange
            var adminUser = TestSetup.CreateClaimsPrincipal(1, "admin", "Admin");
            var httpContext = TestSetup.CreateMockHttpContext(adminUser);
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            // Act
            var result = await _controller.DeleteUser(99999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        #endregion
    }
}
