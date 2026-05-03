namespace Errando.DTOs
{
    public class ClientStatsDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public decimal Rating { get; set; }
        public int TotalReviews { get; set; }
        public int TasksCreated { get; set; }
        public int TasksCompleted { get; set; }
        public int ActiveTasks { get; set; }
        public decimal TotalSpent { get; set; }
        public int ComplaintsFiled { get; set; }
        public decimal CompletionRate { get; set; }
    }
}
