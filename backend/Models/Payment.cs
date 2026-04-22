namespace Errando.Data
{
    public class Payment
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public TodoTask Task { get; set; } = null!;
        public int ClientId { get; set; }
        public User Client { get; set; } = null!;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "usd";
        public string Status { get; set; } = "pending"; // pending, succeeded, failed, refunded
        public string? StripePaymentIntentId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
