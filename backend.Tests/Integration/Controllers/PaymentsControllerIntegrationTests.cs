using Moq;
using Xunit;
using Errando.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Tests.Integration.Controllers
{
    public class PaymentsControllerIntegrationTests : IAsyncLifetime
    {
        private AppDbContext _dbContext = null!;

        public async Task InitializeAsync()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _dbContext = new AppDbContext(options);
            await _dbContext.Database.EnsureCreatedAsync();
            await SeedData();
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
            _dbContext.Tasks.Add(task);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DisposeAsync()
        {
            await _dbContext.DisposeAsync();
        }

        [Fact]
        public async Task Payment_ShouldBeStoredAndRetrievedFromDatabase()
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

            // Act
            _dbContext.Payments.Add(payment);
            await _dbContext.SaveChangesAsync();

            var retrievedPayment = await _dbContext.Payments.FirstOrDefaultAsync(p => p.Id == 1);

            // Assert
            Assert.NotNull(retrievedPayment);
            Assert.Equal(100m, retrievedPayment.Amount);
            Assert.Equal("succeeded", retrievedPayment.Status);
            Assert.Equal("usd", retrievedPayment.Currency);
        }

        [Fact]
        public async Task GetPaymentsByClient_ShouldReturnOnlyClientPayments()
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

            // Act
            _dbContext.Payments.AddRange(payments);
            await _dbContext.SaveChangesAsync();

            var clientPayments = await _dbContext.Payments
                .Where(p => p.ClientId == 1)
                .ToListAsync();

            // Assert
            Assert.Single(clientPayments);
            Assert.Equal(1, clientPayments[0].ClientId);
            Assert.Equal(100m, clientPayments[0].Amount);
        }

        [Fact]
        public async Task GetPaymentHistory_ShouldReturnMultiplePaymentsForTask()
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

            // Act
            _dbContext.Payments.AddRange(payments);
            await _dbContext.SaveChangesAsync();

            var taskPayments = await _dbContext.Payments
                .Where(p => p.TaskId == 1)
                .ToListAsync();

            // Assert
            Assert.Equal(2, taskPayments.Count);
            Assert.All(taskPayments, p => Assert.Equal(1, p.TaskId));
        }

        [Fact]
        public async Task Payment_StatusShouldBeUpdatable()
        {
            // Arrange
            var payment = new Payment
            {
                Id = 1,
                TaskId = 1,
                ClientId = 1,
                Amount = 100m,
                Currency = "usd",
                Status = "pending",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _dbContext.Payments.Add(payment);
            await _dbContext.SaveChangesAsync();

            // Act
            var retrieved = await _dbContext.Payments.FirstOrDefaultAsync(p => p.Id == 1);
            Assert.NotNull(retrieved);
            
            retrieved.Status = "succeeded";
            retrieved.StripePaymentIntentId = "pi_updated_123";
            retrieved.UpdatedAt = DateTime.UtcNow;
            
            _dbContext.Payments.Update(retrieved);
            await _dbContext.SaveChangesAsync();

            var updated = await _dbContext.Payments.FirstOrDefaultAsync(p => p.Id == 1);

            // Assert
            Assert.NotNull(updated);
            Assert.Equal("succeeded", updated.Status);
            Assert.Equal("pi_updated_123", updated.StripePaymentIntentId);
        }

        [Fact]
        public async Task Payment_ShouldHaveValidRelationshipsWithTaskAndClient()
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

            // Act
            _dbContext.Payments.Add(payment);
            await _dbContext.SaveChangesAsync();

            var retrieved = await _dbContext.Payments
                .Include(p => p.Task)
                .Include(p => p.Client)
                .FirstOrDefaultAsync(p => p.Id == 1);

            // Assert
            Assert.NotNull(retrieved);
            Assert.NotNull(retrieved.Task);
            Assert.Equal("Test Task", retrieved.Task.Title);
            Assert.NotNull(retrieved.Client);
            Assert.Equal("testclient", retrieved.Client.Username);
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
