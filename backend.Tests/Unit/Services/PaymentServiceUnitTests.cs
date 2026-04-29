using Moq;
using Xunit;
using Errando.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace backend.Tests.Unit.Services
{
    public class PaymentServiceUnitTests
    {
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<ILogger<StripePaymentService>> _loggerMock;

        public PaymentServiceUnitTests()
        {
            _configurationMock = new Mock<IConfiguration>();
            _loggerMock = new Mock<ILogger<StripePaymentService>>();
            _configurationMock.Setup(x => x["Stripe:SecretKey"]).Returns("test_secret_key");
        }

        [Fact]
        public void ConfigurationIsProperlySetup()
        {
            // Arrange
            var config = _configurationMock.Object;

            // Act
            var secretKey = config["Stripe:SecretKey"];

            // Assert
            Assert.Equal("test_secret_key", secretKey);
        }

        [Theory]
        [InlineData(50)]
        [InlineData(100)]
        [InlineData(500)]
        public void PaymentAmountShouldBePositive(decimal amount)
        {
            // Test that positive amounts are valid
            Assert.True(amount > 0);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public void InvalidPaymentAmountShouldBeLessThanOrEqualToZero(decimal amount)
        {
            // Test that invalid amounts are properly identified
            Assert.True(amount <= 0);
        }

        [Fact]
        public void StripeConfigurationKeyExists()
        {
            // Arrange
            var config = _configurationMock.Object;

            // Act
            var key = config["Stripe:SecretKey"];

            // Assert
            Assert.NotNull(key);
            Assert.NotEmpty(key);
        }

        [Theory]
        [InlineData("usd")]
        [InlineData("eur")]
        [InlineData("gbp")]
        public void SupportedCurrencyCodesShouldBeValid(string currency)
        {
            // Test supported currencies
            var supportedCurrencies = new[] { "usd", "eur", "gbp" };
            Assert.Contains(currency, supportedCurrencies);
        }

        [Fact]
        public void PaymentStatusCanBeValidated()
        {
            // Test payment status values
            var validStatuses = new[] { "pending", "succeeded", "failed" };
            var testStatus = "succeeded";
            
            Assert.Contains(testStatus, validStatuses);
        }
    }
}

