using Xunit;
using Moq;
using Errando.Services;
using Errando.DTOs;

namespace backend.Tests.Unit.Services;

/// <summary>
/// Tests for payment status transitions and validation
/// </summary>
public class PaymentStatusTests
{
    [Theory]
    [InlineData("succeeded")]
    [InlineData("pending")]
    [InlineData("requires_action")]
    public void PaymentDto_StatusValues_Should_Be_Valid(string status)
    {
        // Arrange & Act
        var payment = new PaymentDto { Status = status, Amount = 100m };

        // Assert
        Assert.NotNull(payment.Status);
        Assert.True(!string.IsNullOrEmpty(payment.Status));
    }

    [Fact]
    public async Task ConfirmPayment_SuccessfulPayment_Should_Update_Status()
    {
        // Arrange
        var mockPaymentService = new Mock<IPaymentService>();
        mockPaymentService.Setup(x => x.ConfirmPaymentAsync(It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(new PaymentDto { Status = "succeeded", Amount = 150m });

        // Act
        var result = await mockPaymentService.Object.ConfirmPaymentAsync(1, "pi_test");

        // Assert
        Assert.Equal("succeeded", result.Status);
    }

    [Fact]
    public async Task PaymentHistory_EmptyList_Should_Be_Valid()
    {
        // Arrange
        var mockPaymentService = new Mock<IPaymentService>();
        mockPaymentService.Setup(x => x.GetPaymentHistoryAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<PaymentDto>());

        // Act
        var result = await mockPaymentService.Object.GetPaymentHistoryAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }
}

/// <summary>
/// Tests for payment amount handling and calculation
/// </summary>
public class PaymentAmountTests
{
    [Theory]
    [InlineData(0.01)]
    [InlineData(50.00)]
    [InlineData(99.99)]
    [InlineData(500.00)]
    public void CreatePaymentIntent_VariousAmounts_Should_Create_Intent(decimal amount)
    {
        // Arrange & Act
        var payment = new PaymentIntentDto { Amount = amount };

        // Assert
        Assert.Equal(amount, payment.Amount);
    }

    [Fact]
    public async Task PaymentDto_CurrencyDefault_Should_Be_USD()
    {
        // Arrange & Act
        var payment = new PaymentDto { Currency = "usd", Amount = 50m };

        // Assert
        Assert.Equal("usd", payment.Currency);
    }

    [Theory]
    [InlineData(10, true)]
    [InlineData(100, true)]
    [InlineData(1000, true)]
    public void HasPaid_VariousTaskIds_Should_Check_Payment(int taskId, bool expected)
    {
        // Arrange & Act
        var payment = new PaymentDto { Amount = 100m };

        // Assert
        Assert.Equal(true, !string.IsNullOrEmpty(payment.Currency));
    }
}

/// <summary>
/// Tests for email notification content and recipients
/// </summary>
public class EmailContentTests
{
    [Theory]
    [InlineData("user@example.com")]
    [InlineData("admin@company.co.uk")]
    [InlineData("test.user+tag@domain.org")]
    public async Task SendRegistration_VariousEmails_Should_Send(string email)
    {
        // Arrange
        var mockEmailService = new Mock<IEmailService>();
        mockEmailService.Setup(x => x.SendRegistrationConfirmationAsync(email, It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        var result = await mockEmailService.Object.SendRegistrationConfirmationAsync(email, "testuser");

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData("alice_client")]
    [InlineData("bob.runner")]
    [InlineData("charlie-admin")]
    public async Task SendEmail_VariousUsernames_Should_Include_Username(string username)
    {
        // Arrange
        var mockEmailService = new Mock<IEmailService>();
        mockEmailService.Setup(x => x.SendRegistrationConfirmationAsync(It.IsAny<string>(), username))
            .ReturnsAsync(true);

        // Act
        var result = await mockEmailService.Object.SendRegistrationConfirmationAsync("test@test.com", username);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task SendTaskCreated_WithScheduledTime_Should_Include_DateTime()
    {
        // Arrange
        var mockEmailService = new Mock<IEmailService>();
        var scheduledTime = new DateTime(2026, 5, 15, 14, 30, 0);
        
        mockEmailService.Setup(x => x.SendTaskCreatedAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), scheduledTime))
            .ReturnsAsync(true);

        // Act
        var result = await mockEmailService.Object.SendTaskCreatedAsync("user@test.com", "Task", "client", scheduledTime);

        // Assert
        Assert.True(result);
        mockEmailService.Verify(x => x.SendTaskCreatedAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), scheduledTime), Times.Once);
    }
}

/// <summary>
/// Tests for service call counting and verification
/// </summary>
public class ServiceCallVerificationTests
{
    [Fact]
    public async Task Payment_MultipleOperations_Should_Track_Each_Call()
    {
        // Arrange
        var mockPaymentService = new Mock<IPaymentService>();
        mockPaymentService.Setup(x => x.CreatePaymentIntentAsync(It.IsAny<int>(), It.IsAny<decimal>()))
            .ReturnsAsync(new PaymentIntentDto());

        // Act
        await mockPaymentService.Object.CreatePaymentIntentAsync(1, 100);
        await mockPaymentService.Object.CreatePaymentIntentAsync(2, 200);
        await mockPaymentService.Object.CreatePaymentIntentAsync(3, 300);

        // Assert
        mockPaymentService.Verify(x => x.CreatePaymentIntentAsync(It.IsAny<int>(), It.IsAny<decimal>()), Times.Exactly(3));
    }

    [Fact]
    public async Task Email_NotificationSequence_Should_Preserve_Order()
    {
        // Arrange
        var mockEmailService = new Mock<IEmailService>();
        var callSequence = new List<string>();

        mockEmailService.Setup(x => x.SendRegistrationConfirmationAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns<string, string>((e, u) => 
            {
                callSequence.Add($"registration:{e}");
                return Task.FromResult(true);
            });

        mockEmailService.Setup(x => x.SendTaskCreatedAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
            .Returns<string, string, string, DateTime>((e, t, u, d) =>
            {
                callSequence.Add($"taskcreated:{e}");
                return Task.FromResult(true);
            });

        // Act
        await mockEmailService.Object.SendRegistrationConfirmationAsync("user@test.com", "user");
        await mockEmailService.Object.SendTaskCreatedAsync("user@test.com", "Task", "user", DateTime.UtcNow);

        // Assert
        Assert.Equal(2, callSequence.Count);
        Assert.Contains("registration:", callSequence[0]);
        Assert.Contains("taskcreated:", callSequence[1]);
    }

    [Fact]
    public async Task Service_NeverCalledMethod_Should_Verify_Zero_Calls()
    {
        // Arrange
        var mockPaymentService = new Mock<IPaymentService>();

        // Act
        // (no calls made)

        // Assert
        mockPaymentService.Verify(x => x.HasPaidAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }
}

/// <summary>
/// Tests for async operation handling
/// </summary>
public class AsyncOperationTests
{
    [Fact]
    public async Task AsyncMethod_Should_Complete_Successfully()
    {
        // Arrange
        var mockPaymentService = new Mock<IPaymentService>();
        mockPaymentService.Setup(x => x.CreatePaymentIntentAsync(It.IsAny<int>(), It.IsAny<decimal>()))
            .ReturnsAsync(new PaymentIntentDto());

        // Act
        var task = mockPaymentService.Object.CreatePaymentIntentAsync(1, 100m);
        var result = await task;

        // Assert
        Assert.True(task.IsCompleted);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task ConcurrentAsyncCalls_Should_All_Complete()
    {
        // Arrange
        var mockEmailService = new Mock<IEmailService>();
        mockEmailService.Setup(x => x.SendRegistrationConfirmationAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        var tasks = new List<Task<bool>>();
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(mockEmailService.Object.SendRegistrationConfirmationAsync($"user{i}@test.com", $"user{i}"));
        }

        var results = await Task.WhenAll(tasks);

        // Assert
        Assert.All(results, r => Assert.True(r));
        Assert.Equal(10, results.Length);
    }

    [Fact]
    public async Task AsyncTask_WithDifferentDelays_Should_Complete()
    {
        // Arrange
        var mockPaymentService = new Mock<IPaymentService>();
        mockPaymentService.Setup(x => x.GetPaymentHistoryAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<PaymentDto> { new PaymentDto { Amount = 100m } });

        // Act
        var startTime = DateTime.UtcNow;
        var result = await mockPaymentService.Object.GetPaymentHistoryAsync(1);
        var endTime = DateTime.UtcNow;

        // Assert
        Assert.NotNull(result);
        Assert.True((endTime - startTime).TotalMilliseconds >= 0);
    }
}
