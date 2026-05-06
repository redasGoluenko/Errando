using Moq;
using Xunit;
using Errando.Data;
using Errando.Controllers;
using Errando.Services;
using Errando.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

namespace backend.Tests.Integration.Controllers
{
    public class PaymentsControllerIntegrationTests : IAsyncLifetime
    {
        private AppDbContext _dbContext = null!;
        private PaymentsController _controller = null!;
        private Mock<IPaymentService> _paymentServiceMock = null!;
        private Mock<ILogger<PaymentsController>> _loggerMock = null!;

        public async Task InitializeAsync()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _dbContext = new AppDbContext(options);
            await _dbContext.Database.EnsureCreatedAsync();
            await SeedData();

            _paymentServiceMock = new Mock<IPaymentService>();
            _loggerMock = new Mock<ILogger<PaymentsController>>();

            _controller = new PaymentsController(_paymentServiceMock.Object, _dbContext, _loggerMock.Object);
            
            // Setup default user claims
            SetupUserClaims(userId: 1);
        }

        private void SetupUserClaims(int userId)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, "Client")
            };
            
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = principal } };
        }

        private async Task SeedData()
        {
            var client = new User
            {
                Id = 1,
                Username = "testclient",
                Email = "client@test.com",
                PasswordHash = "hashed",
                Role = "Client",
                AverageRating = 4.5m,
                TotalReviews = 10
            };

            var runner = new User
            {
                Id = 2,
                Username = "testrunner",
                Email = "runner@test.com",
                PasswordHash = "hashed",
                Role = "Runner",
                AverageRating = 4.8m,
                TotalReviews = 15
            };

            var task = new TodoTask
            {
                Id = 1,
                Title = "Test Task",
                Description = "Description",
                ScheduledTime = DateTime.UtcNow.AddDays(1),
                Status = "pending",
                ClientId = 1,
                Location = "Remote",
                Price = 100m,
                IsRecurring = false,
                IsExpired = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _dbContext.Users.Add(client);
            _dbContext.Users.Add(runner);
            _dbContext.Tasks.Add(task);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DisposeAsync()
        {
            await _dbContext.DisposeAsync();
        }

        [Fact]
        public async Task CreatePaymentIntent_ShouldReturnBadRequest_WhenTaskNotFound()
        {
            // Arrange
            var request = new CreatePaymentDto { TaskId = 999, Amount = 100m };
            
            // Act
            var result = await _controller.CreatePaymentIntent(request);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task CreatePaymentIntent_ShouldReturnForbid_WhenUserIsNotTaskOwner()
        {
            // Arrange
            SetupUserClaims(userId: 2); // Different user
            var request = new CreatePaymentDto { TaskId = 1, Amount = 100m };
            
            // Act
            var result = await _controller.CreatePaymentIntent(request);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task CreatePaymentIntent_ShouldReturnOk_WhenValidRequest()
        {
            // Arrange
            var request = new CreatePaymentDto { TaskId = 1, Amount = 100m };
            
            _paymentServiceMock
                .Setup(x => x.CreatePaymentIntentAsync(It.IsAny<int>(), It.IsAny<decimal>()))
                .ReturnsAsync(new PaymentDto 
                { 
                    Id = 1, 
                    Amount = 100m, 
                    Status = "requires_payment_method",
                    ClientId = 1,
                    TaskId = 1,
                    Currency = "usd"
                });

            // Act
            var result = await _controller.CreatePaymentIntent(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            _paymentServiceMock.Verify(x => x.CreatePaymentIntentAsync(1, 100m), Times.Once);
        }

        [Fact]
        public async Task GetPaymentHistory_ShouldReturnUnauthorized_WhenNoUser()
        {
            // Arrange
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal();
            
            // Act
            var result = await _controller.GetPaymentHistory(1);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task GetPaymentHistory_ShouldReturnOk_WhenUserIsTaskOwner()
        {
            // Arrange
            var payments = new List<PaymentDto>
            {
                new PaymentDto { Id = 1, TaskId = 1, ClientId = 1, Amount = 100m, Status = "succeeded", Currency = "usd" }
            };

            _paymentServiceMock
                .Setup(x => x.GetPaymentHistoryByTaskAsync(It.IsAny<int>()))
                .ReturnsAsync(payments);

            // Act
            var result = await _controller.GetPaymentHistory(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedPayments = Assert.IsAssignableFrom<IEnumerable<PaymentDto>>(okResult.Value);
            Assert.Single(returnedPayments);
            _paymentServiceMock.Verify(x => x.GetPaymentHistoryByTaskAsync(1), Times.Once);
        }

        [Fact]
        public async Task IsTaskPaid_ShouldReturnTrue_WhenPaymentSucceeded()
        {
            // Arrange
            _paymentServiceMock
                .Setup(x => x.IsTaskPaidAsync(It.IsAny<int>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.IsTaskPaid(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)okResult.Value);
            _paymentServiceMock.Verify(x => x.IsTaskPaidAsync(1), Times.Once);
        }

        [Fact]
        public async Task IsTaskPaid_ShouldReturnFalse_WhenPaymentNotCompleted()
        {
            // Arrange
            _paymentServiceMock
                .Setup(x => x.IsTaskPaidAsync(It.IsAny<int>()))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.IsTaskPaid(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.False((bool)okResult.Value);
        }

        [Fact]
        public async Task GetMyPayments_ShouldReturnUserPayments()
        {
            // Arrange
            var payments = new List<PaymentDto>
            {
                new PaymentDto { Id = 1, TaskId = 1, ClientId = 1, Amount = 100m, Status = "succeeded", Currency = "usd" },
                new PaymentDto { Id = 2, TaskId = 2, ClientId = 1, Amount = 50m, Status = "pending", Currency = "usd" }
            };

            _paymentServiceMock
                .Setup(x => x.GetPaymentsByClientAsync(It.IsAny<int>()))
                .ReturnsAsync(payments);

            // Act
            var result = await _controller.GetMyPayments();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedPayments = Assert.IsAssignableFrom<IEnumerable<PaymentDto>>(okResult.Value);
            Assert.Equal(2, returnedPayments.Count());
            _paymentServiceMock.Verify(x => x.GetPaymentsByClientAsync(1), Times.Once);
        }


        [Fact]
        public async Task MultiplePayments_ShouldBeSeparateRecords()
        {
            // Arrange
            var payment1 = new Payment
            {
                Id = 1,
                TaskId = 1,
                ClientId = 1,
                Amount = 50m,
                Currency = "usd",
                Status = "pending",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var payment2 = new Payment
            {
                Id = 2,
                TaskId = 1,
                ClientId = 1,
                Amount = 25m,
                Currency = "usd",
                Status = "succeeded",
                StripePaymentIntentId = "pi_test_2",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Act
            _dbContext.Payments.AddRange(payment1, payment2);
            await _dbContext.SaveChangesAsync();

            var allPayments = await _dbContext.Payments.ToListAsync();

            // Assert
            Assert.Equal(2, allPayments.Count);
            Assert.NotEqual(allPayments[0].Amount, allPayments[1].Amount);
            Assert.Equal(50m, allPayments.First(p => p.Id == 1).Amount);
            Assert.Equal(25m, allPayments.First(p => p.Id == 2).Amount);
        }
    }
}
