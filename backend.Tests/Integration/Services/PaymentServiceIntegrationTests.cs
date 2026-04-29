using Moq;
using Xunit;
using Errando.Services;
using Errando.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace backend.Tests.Integration.Services
{
    public class PaymentServiceIntegrationTests : IAsyncLifetime
    {
        private AppDbContext _context = null!;
        private Mock<IConfiguration> _configMock = null!;
        private Mock<ILogger<StripePaymentService>> _loggerMock = null!;
        private StripePaymentService _paymentService = null!;

        public async Task InitializeAsync()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            await _context.Database.EnsureCreatedAsync();

            _configMock = new Mock<IConfiguration>();
            _loggerMock = new Mock<ILogger<StripePaymentService>>();
            
            _configMock.Setup(x => x["Stripe:SecretKey"]).Returns("sk_test_fake");
            
            _paymentService = new StripePaymentService(_context, _configMock.Object, _loggerMock.Object);

            // Seed test data
            await SeedTestData();
        }

        private async Task SeedTestData()
        {
            // Create test users
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

            // Create test task
            var task = new TodoTask
            {
                Id = 1,
                Title = "Design Logo",
                Description = "Create a modern logo",
                ScheduledTime = DateTime.UtcNow.AddDays(1),
                Status = "pending",
                ClientId = 1,
                RunnerId = null,
                Location = "Remote",
                Price = 100m,
                IsRecurring = false,
                IsExpired = false,
                IsDeletedByClient = false,
                IsDeletedByRunner = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.AddRange(client, runner);
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
        }

        public async Task DisposeAsync()
        {
            await _context.DisposeAsync();
        }

        [Fact]
        public async Task HasPaidAsync_WithNoPriorPayments_ShouldReturnFalse()
        {
            // Arrange
            int taskId = 1;
            int clientId = 1;

            // Act
            var result = await _paymentService.HasPaidAsync(taskId, clientId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task HasPaidAsync_WithSuccessfulPayment_ShouldReturnTrue()
        {
            // Arrange
            var payment = new Payment
            {
                Id = 1,
                TaskId = 1,
                ClientId = 1,
                Amount = 100m,
                Currency = "usd",
                Status = "succeeded",
                StripePaymentIntentId = "pi_test_123",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _paymentService.HasPaidAsync(1, 1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task GetPaymentHistoryAsync_ShouldReturnPaymentsForTask()
        {
            // Arrange
            var payments = new List<Payment>
            {
                new Payment 
                { 
                    Id = 1, 
                    TaskId = 1, 
                    ClientId = 1, 
                    Amount = 50m, 
                    Currency = "usd", 
                    Status = "succeeded",
                    StripePaymentIntentId = "pi_test_1",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Payment 
                { 
                    Id = 2, 
                    TaskId = 1, 
                    ClientId = 1, 
                    Amount = 50m, 
                    Currency = "usd", 
                    Status = "succeeded",
                    StripePaymentIntentId = "pi_test_2",
                    CreatedAt = DateTime.UtcNow.AddHours(-1),
                    UpdatedAt = DateTime.UtcNow.AddHours(-1)
                }
            };

            _context.Payments.AddRange(payments);
            await _context.SaveChangesAsync();

            // Act
            var result = await _paymentService.GetPaymentHistoryAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, p => Assert.Equal(1, p.TaskId));
        }

        [Fact]
        public async Task GetPaymentsByClientAsync_ShouldReturnOnlyClientPayments()
        {
            // Arrange
            var payments = new List<Payment>
            {
                new Payment 
                { 
                    Id = 1, 
                    TaskId = 1, 
                    ClientId = 1, 
                    Amount = 100m, 
                    Currency = "usd", 
                    Status = "succeeded",
                    StripePaymentIntentId = "pi_test_1",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Payment 
                { 
                    Id = 2, 
                    TaskId = 2, 
                    ClientId = 2, 
                    Amount = 200m, 
                    Currency = "usd", 
                    Status = "succeeded",
                    StripePaymentIntentId = "pi_test_2",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            _context.Payments.AddRange(payments);
            await _context.SaveChangesAsync();

            // Act
            var result = await _paymentService.GetPaymentsByClientAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(1, result[0].ClientId);
            Assert.Equal(100m, result[0].Amount);
        }

        [Fact]
        public async Task CreatePaymentIntentAsync_WithInvalidTaskId_ShouldThrowException()
        {
            // Arrange
            int invalidTaskId = 9999;
            decimal amount = 100m;

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _paymentService.CreatePaymentIntentAsync(invalidTaskId, amount)
            );
        }
    }
}
