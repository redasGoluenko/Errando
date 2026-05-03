namespace Errando.DTOs
{
    public class RunnerStatsDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public decimal Rating { get; set; }
        public int TotalReviews { get; set; }
        public int TasksCompleted { get; set; }
        public int ActiveTasks { get; set; }
        public decimal MoneyEarned { get; set; }
        public decimal TaskAcceptanceRate { get; set; }
        public int TotalTasksAssigned { get; set; }
    }
}
