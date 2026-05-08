using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Errando.Data;
using Errando.DTOs;
using Errando.Services;
using backend.Tests.Helpers;
using Errando.Controllers;

namespace backend.Tests.Integration.Controllers
{
    public class PaymentsControllerTests : IAsyncLifetime
    {
        private AppDbContext _context = null!;
        private Mock<IPaymentService> _paymentServiceMock = null!;
        private Mock<ILogger<PaymentsController>> _loggerMock = null!;
        private PaymentsController _controller = null!;

        public async Task InitializeAsync()
        {
            _context = TestSetup.CreateInMemoryContext();
            _paymentServiceMock = new Mock<IPaymentService>();
            _loggerMock = new Mock<ILogger<PaymentsController>>();
            _controller = new PaymentsController(_paymentServiceMock.Object, _context, _loggerMock.Object);
            await TestSetup.SeedTestDataAsync(_context);
        }

        public async Task DisposeAsync()
        {
            await _context.DisposeAsync();
        }

        [Fact]
        [Trait("Category", "Integration")]
        [Trait("Feature", "Payments")]
        public async Task CreatePaymentIntent_WithValidTask_ReturnsPaymentIntent()
        {
            // Arrange
            var client = await _context.Users.FirstAsync(u => u.Role == "Client");
            var task = new TodoTask { ClientId = client.Id, Title = "Payment Task", Status = "Pending", CreatedAt = DateTime.UtcNow };
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            var claims = TestSetup.CreateClaimsPrincipal(client.Id, client.Username, "Client");
            _controller.ControllerContext.HttpContext = TestSetup.CreateMockHttpContext(claims);

            var paymentIntentDto = new PaymentIntentDto
            {
                PaymentId = 1,
                ClientSecret = "test_secret",
                PaymentIntentId = "pi_test",
                Amount = 100m,
                Currency = "usd"
            };

            _paymentServiceMock.Setup(x => x.CreatePaymentIntentAsync(task.Id, 100m))
                .ReturnsAsync(paymentIntentDto);
            _paymentServiceMock.Setup(x => x.HasPaidAsync(task.Id, client.Id))
                .ReturnsAsync(false);

            var request = new CreatePaymentDto { TaskId = task.Id, Amount = 100m };

            // Act
            var result = await _controller.CreatePaymentIntent(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        [Trait("Category", "Integration")]
        [Trait("Feature", "Payments")]
        public async Task CreatePaymentIntent_WithNonExistentTask_ReturnsNotFound()
        {
            // Arrange
            var client = await _context.Users.FirstAsync(u => u.Role == "Client");
            var claims = TestSetup.CreateClaimsPrincipal(client.Id, client.Username, "Client");
            _controller.ControllerContext.HttpContext = TestSetup.CreateMockHttpContext(claims);

            var request = new CreatePaymentDto { TaskId = 999, Amount = 100m };

            // Act
            var result = await _controller.CreatePaymentIntent(request);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        [Trait("Category", "Integration")]
        [Trait("Feature", "Payments")]
        public async Task CreatePaymentIntent_WithAlreadyPaidTask_ReturnsBadRequest()
        {
            // Arrange
            var client = await _context.Users.FirstAsync(u => u.Role == "Client");
            var task = new TodoTask { ClientId = client.Id, Title = "Paid Task", Status = "Pending", CreatedAt = DateTime.UtcNow };
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            var claims = TestSetup.CreateClaimsPrincipal(client.Id, client.Username, "Client");
            _controller.ControllerContext.HttpContext = TestSetup.CreateMockHttpContext(claims);

            _paymentServiceMock.Setup(x => x.HasPaidAsync(task.Id, client.Id))
                .ReturnsAsync(true); // Already paid

            var request = new CreatePaymentDto { TaskId = task.Id, Amount = 100m };

            // Act
            var result = await _controller.CreatePaymentIntent(request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        [Trait("Category", "Integration")]
        [Trait("Feature", "Payments")]
        public async Task CreatePaymentIntent_WithOtherUsersTask_ReturnsForbid()
        {
            // Arrange
            var client1 = await _context.Users.FirstAsync(u => u.Role == "Client");
            var admin = await _context.Users.FirstAsync(u => u.Role == "Admin");
            
            var task = new TodoTask { ClientId = admin.Id, Title = "Admin Task", Status = "Pending", CreatedAt = DateTime.UtcNow };
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            var claims = TestSetup.CreateClaimsPrincipal(client1.Id, client1.Username, "Client");
            _controller.ControllerContext.HttpContext = TestSetup.CreateMockHttpContext(claims);

            var request = new CreatePaymentDto { TaskId = task.Id, Amount = 100m };

            // Act
            var result = await _controller.CreatePaymentIntent(request);

            // Assert
            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        [Trait("Category", "Integration")]
        [Trait("Feature", "Payments")]
        public async Task ConfirmPayment_WithValidData_ReturnsPayment()
        {
            // Arrange
            var client = await _context.Users.FirstAsync(u => u.Role == "Client");
            var claims = TestSetup.CreateClaimsPrincipal(client.Id, client.Username, "Client");
            _controller.ControllerContext.HttpContext = TestSetup.CreateMockHttpContext(claims);

            var paymentDto = new PaymentDto
            {
                Id = 1,
                TaskId = 1,
                ClientId = client.Id,
                Amount = 100m,
                Currency = "usd",
                Status = "succeeded"
            };

            _paymentServiceMock.Setup(x => x.ConfirmPaymentAsync(1, "pi_test"))
                .ReturnsAsync(paymentDto);

            var request = new ConfirmPaymentDto { PaymentId = 1, PaymentIntentId = "pi_test" };

            // Act
            var result = await _controller.ConfirmPayment(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        [Trait("Category", "Integration")]
        [Trait("Feature", "Payments")]
        public async Task GetPaymentHistory_ReturnsPayments()
        {
            // Arrange
            var client = await _context.Users.FirstAsync(u => u.Role == "Client");
            var claims = TestSetup.CreateClaimsPrincipal(client.Id, client.Username, "Client");
            _controller.ControllerContext.HttpContext = TestSetup.CreateMockHttpContext(claims);

            var payments = new List<PaymentDto>
            {
                new PaymentDto { Id = 1, TaskId = 1, Amount = 100m, Status = "succeeded" }
            };

            _paymentServiceMock.Setup(x => x.GetPaymentHistoryAsync(1))
                .ReturnsAsync(payments);

            // Act
            var result = await _controller.GetPaymentHistory(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedPayments = Assert.IsAssignableFrom<List<PaymentDto>>(okResult.Value);
            Assert.NotEmpty(returnedPayments);
        }
    }
}
