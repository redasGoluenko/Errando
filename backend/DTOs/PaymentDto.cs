namespace Errando.DTOs
{
    public class PaymentDto
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public int ClientId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "usd";
        public string Status { get; set; } = "pending";
        public string? StripePaymentIntentId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreatePaymentDto
    {
        public int TaskId { get; set; }
        public decimal Amount { get; set; }
    }

    public class PaymentIntentDto
    {
        public int PaymentId { get; set; }
        public string ClientSecret { get; set; } = null!;
        public string PaymentIntentId { get; set; } = null!;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "usd";
    }

    public class ConfirmPaymentDto
    {
        public int PaymentId { get; set; }
        public string PaymentIntentId { get; set; } = null!;
    }
}
