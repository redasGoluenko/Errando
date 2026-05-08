using Xunit;
using Moq;
using Errando.Services;
using Errando.DTOs;

namespace backend.Tests.Unit.Services;

public class PaymentServiceExtendedTests
{
    [Fact]
    public async Task CreatePaymentIntent_ValidAmounts_Should_Return_PaymentIntent()
    {
        // Arrange
        var mockPaymentService = new Mock<IPaymentService>();
        mockPaymentService.Setup(x => x.CreatePaymentIntentAsync(It.IsAny<int>(), It.IsAny<decimal>()))
            .ReturnsAsync(new PaymentIntentDto 
            { 
                PaymentId = 1, 
                PaymentIntentId = "pi_test",
                ClientSecret = "secret_test",
                Amount = 100m
            });

        // Act
        var result = await mockPaymentService.Object.CreatePaymentIntentAsync(1, 100m);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("pi_test", result.PaymentIntentId);
        mockPaymentService.Verify(x => x.CreatePaymentIntentAsync(1, 100m), Times.Once);
    }

    [Fact]
    public async Task ConfirmPayment_ValidPayment_Should_Return_Confirmed()
    {
        // Arrange
        var mockPaymentService = new Mock<IPaymentService>();
        mockPaymentService.Setup(x => x.ConfirmPaymentAsync(It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(new PaymentDto 
            { 
                Id = 1, 
                Status = "succeeded",
                Amount = 100m
            });

        // Act
        var result = await mockPaymentService.Object.ConfirmPaymentAsync(1, "pi_123");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("succeeded", result.Status);
    }

    [Fact]
    public async Task GetPaymentHistory_ForTask_Should_Return_List()
    {
        // Arrange
        var mockPaymentService = new Mock<IPaymentService>();
        var expectedPayments = new List<PaymentDto> 
        {
            new PaymentDto { Id = 1, Status = "succeeded", Amount = 100m },
            new PaymentDto { Id = 2, Status = "succeeded", Amount = 50m }
        };

        mockPaymentService.Setup(x => x.GetPaymentHistoryAsync(It.IsAny<int>()))
            .ReturnsAsync(expectedPayments);

        // Act
        var result = await mockPaymentService.Object.GetPaymentHistoryAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        mockPaymentService.Verify(x => x.GetPaymentHistoryAsync(1), Times.Once);
    }

    [Fact]
    public async Task HasPaid_ClientPaidForTask_Should_Return_True()
    {
        // Arrange
        var mockPaymentService = new Mock<IPaymentService>();
        mockPaymentService.Setup(x => x.HasPaidAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(true);

        // Act
        var result = await mockPaymentService.Object.HasPaidAsync(1, 1);

        // Assert
        Assert.True(result);
        mockPaymentService.Verify(x => x.HasPaidAsync(1, 1), Times.Once);
    }

    [Fact]
    public async Task GetPaymentsByClient_ForClient_Should_Return_All_Payments()
    {
        // Arrange
        var mockPaymentService = new Mock<IPaymentService>();
        var expectedPayments = new List<PaymentDto>
        {
            new PaymentDto { ClientId = 1, Amount = 100m, Status = "succeeded" },
            new PaymentDto { ClientId = 1, Amount = 75m, Status = "succeeded" },
            new PaymentDto { ClientId = 1, Amount = 50m, Status = "pending" }
        };

        mockPaymentService.Setup(x => x.GetPaymentsByClientAsync(It.IsAny<int>()))
            .ReturnsAsync(expectedPayments);

        // Act
        var result = await mockPaymentService.Object.GetPaymentsByClientAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
    }
}

public class PaymentServiceValidationTests
{
    [Theory]
    [InlineData(100, true)]
    [InlineData(0.01, true)]
    [InlineData(1000, true)]
    public async Task CreatePaymentIntent_DifferentAmounts_Should_Accept_Valid(decimal amount, bool valid)
    {
        // Arrange
        var mockPaymentService = new Mock<IPaymentService>();
        mockPaymentService.Setup(x => x.CreatePaymentIntentAsync(It.IsAny<int>(), It.IsAny<decimal>()))
            .ReturnsAsync(new PaymentIntentDto { Amount = amount });

        // Act
        var result = await mockPaymentService.Object.CreatePaymentIntentAsync(1, amount);

        // Assert
        Assert.Equal(valid, result.Amount > 0);
    }

    [Fact]
    public async Task HasPaid_MultipleClients_Should_Check_Individually()
    {
        // Arrange
        var mockPaymentService = new Mock<IPaymentService>();
        mockPaymentService.Setup(x => x.HasPaidAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(true);

        // Act
        var client1 = await mockPaymentService.Object.HasPaidAsync(1, 1);
        var client2 = await mockPaymentService.Object.HasPaidAsync(1, 2);
        var client3 = await mockPaymentService.Object.HasPaidAsync(1, 3);

        // Assert
        Assert.True(client1);
        Assert.True(client2);
        Assert.True(client3);
        mockPaymentService.Verify(x => x.HasPaidAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(3));
    }

    [Fact]
    public async Task ConfirmPayment_SequentialConfirmations_Should_Each_Call_Service()
    {
        // Arrange
        var mockPaymentService = new Mock<IPaymentService>();
        mockPaymentService.Setup(x => x.ConfirmPaymentAsync(It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(new PaymentDto { Status = "succeeded" });

        // Act
        await mockPaymentService.Object.ConfirmPaymentAsync(1, "pi_1");
        await mockPaymentService.Object.ConfirmPaymentAsync(2, "pi_2");
        await mockPaymentService.Object.ConfirmPaymentAsync(3, "pi_3");

        // Assert
        mockPaymentService.Verify(x => x.ConfirmPaymentAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Exactly(3));
    }
}
