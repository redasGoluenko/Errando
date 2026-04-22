using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Errando.Services;
using Errando.DTOs;
using Errando.Data;

namespace Errando.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly AppDbContext _context;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(IPaymentService paymentService, AppDbContext context, ILogger<PaymentsController> logger)
        {
            _paymentService = paymentService;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Create a payment intent for a task
        /// </summary>
        [HttpPost("create-intent")]
        public async Task<IActionResult> CreatePaymentIntent([FromBody] CreatePaymentDto request)
        {
            try
            {
                // Get current user ID
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                    return Unauthorized("User not found");

                if (!int.TryParse(userIdClaim.Value, out var userId))
                    return Unauthorized("Invalid user ID");

                // Verify task exists and belongs to user
                var task = await _context.Tasks.FindAsync(request.TaskId);
                if (task == null)
                    return NotFound("Task not found");

                if (task.ClientId != userId)
                    return Forbid("You can only pay for your own tasks");

                // Check if already paid
                var hasPaid = await _paymentService.HasPaidAsync(request.TaskId, userId);
                if (hasPaid)
                    return BadRequest("Task has already been paid");

                var paymentIntent = await _paymentService.CreatePaymentIntentAsync(request.TaskId, request.Amount);
                return Ok(paymentIntent);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating payment intent: {ex.Message}");
                return StatusCode(500, new { error = "Failed to create payment intent" });
            }
        }

        /// <summary>
        /// Confirm a payment after Stripe processing
        /// </summary>
        [HttpPost("confirm")]
        public async Task<IActionResult> ConfirmPayment([FromBody] ConfirmPaymentDto request)
        {
            try
            {
                var payment = await _paymentService.ConfirmPaymentAsync(request.PaymentId, request.PaymentIntentId);
                return Ok(payment);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error confirming payment: {ex.Message}");
                return StatusCode(500, new { error = "Failed to confirm payment" });
            }
        }

        /// <summary>
        /// Get payment history for a task
        /// </summary>
        [HttpGet("task/{taskId}")]
        public async Task<IActionResult> GetPaymentHistory(int taskId)
        {
            try
            {
                var payments = await _paymentService.GetPaymentHistoryAsync(taskId);
                return Ok(payments);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving payment history: {ex.Message}");
                return StatusCode(500, new { error = "Failed to retrieve payment history" });
            }
        }

        /// <summary>
        /// Check if a task has been paid
        /// </summary>
        [HttpGet("{taskId}/paid")]
        public async Task<IActionResult> HasPaid(int taskId)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                    return Unauthorized("User not found");

                if (!int.TryParse(userIdClaim.Value, out var userId))
                    return Unauthorized("Invalid user ID");

                var hasPaid = await _paymentService.HasPaidAsync(taskId, userId);
                return Ok(new { hasPaid });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error checking payment status: {ex.Message}");
                return StatusCode(500, new { error = "Failed to check payment status" });
            }
        }

        /// <summary>
        /// Get all payments for the current user
        /// </summary>
        [HttpGet("my-payments")]
        public async Task<IActionResult> GetMyPayments()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                    return Unauthorized("User not found");

                if (!int.TryParse(userIdClaim.Value, out var userId))
                    return Unauthorized("Invalid user ID");

                var payments = await _paymentService.GetPaymentsByClientAsync(userId);
                return Ok(payments);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving client payments: {ex.Message}");
                return StatusCode(500, new { error = "Failed to retrieve payment history" });
            }
        }
    }
}
