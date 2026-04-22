using Stripe;
using Errando.Data;
using Errando.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Errando.Services
{
    public interface IPaymentService
    {
        Task<PaymentIntentDto> CreatePaymentIntentAsync(int taskId, decimal amount);
        Task<PaymentDto> ConfirmPaymentAsync(int paymentId, string paymentIntentId);
        Task<List<PaymentDto>> GetPaymentHistoryAsync(int taskId);
        Task<bool> HasPaidAsync(int taskId, int clientId);
    }

    public class StripePaymentService : IPaymentService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<StripePaymentService> _logger;

        public StripePaymentService(AppDbContext context, IConfiguration configuration, ILogger<StripePaymentService> logger)
        {
            _context = context;
            _logger = logger;
            StripeConfiguration.ApiKey = configuration["Stripe:SecretKey"];
        }

        public async Task<PaymentIntentDto> CreatePaymentIntentAsync(int taskId, decimal amount)
        {
            try
            {
                // Verify task exists
                var task = await _context.Tasks.FindAsync(taskId);
                if (task == null)
                    throw new InvalidOperationException("Task not found");

                // Create Payment record
                var payment = new Payment
                {
                    TaskId = taskId,
                    ClientId = task.ClientId,
                    Amount = amount,
                    Currency = "usd",
                    Status = "pending"
                };

                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                // Create Stripe PaymentIntent
                var options = new PaymentIntentCreateOptions
                {
                    Amount = (long)(amount * 100), // Stripe uses cents
                    Currency = "usd",
                    Metadata = new Dictionary<string, string>
                    {
                        { "taskId", taskId.ToString() },
                        { "paymentId", payment.Id.ToString() }
                    }
                };

                var service = new PaymentIntentService();
                var paymentIntent = await service.CreateAsync(options);

                // Update payment with Stripe ID
                payment.StripePaymentIntentId = paymentIntent.Id;
                await _context.SaveChangesAsync();

                return new PaymentIntentDto
                {
                    PaymentId = payment.Id,
                    ClientSecret = paymentIntent.ClientSecret,
                    PaymentIntentId = paymentIntent.Id,
                    Amount = amount,
                    Currency = "usd"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating payment intent: {ex.Message}");
                throw;
            }
        }

        public async Task<PaymentDto> ConfirmPaymentAsync(int paymentId, string paymentIntentId)
        {
            try
            {
                var payment = await _context.Payments.FindAsync(paymentId);
                if (payment == null)
                    throw new InvalidOperationException("Payment not found");

                // Retrieve PaymentIntent from Stripe
                var service = new PaymentIntentService();
                var paymentIntent = await service.GetAsync(paymentIntentId);

                // Update payment status
                if (paymentIntent.Status == "succeeded")
                {
                    payment.Status = "succeeded";
                    payment.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
                else if (paymentIntent.Status == "requires_action")
                {
                    payment.Status = "requires_action";
                    payment.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
                else if (paymentIntent.Status == "canceled")
                {
                    payment.Status = "failed";
                    payment.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }

                return MapToDto(payment);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error confirming payment: {ex.Message}");
                throw;
            }
        }

        public async Task<List<PaymentDto>> GetPaymentHistoryAsync(int taskId)
        {
            var payments = await _context.Payments
                .Where(p => p.TaskId == taskId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return payments.Select(MapToDto).ToList();
        }

        public async Task<bool> HasPaidAsync(int taskId, int clientId)
        {
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => 
                    p.TaskId == taskId && 
                    p.ClientId == clientId && 
                    p.Status == "succeeded");

            return payment != null;
        }

        private PaymentDto MapToDto(Payment payment)
        {
            return new PaymentDto
            {
                Id = payment.Id,
                TaskId = payment.TaskId,
                ClientId = payment.ClientId,
                Amount = payment.Amount,
                Currency = payment.Currency,
                Status = payment.Status,
                StripePaymentIntentId = payment.StripePaymentIntentId,
                CreatedAt = payment.CreatedAt,
                UpdatedAt = payment.UpdatedAt
            };
        }
    }
}
