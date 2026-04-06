namespace Errando.DTOs
{
    public class RunnerStatsDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public decimal Rating { get; set; }
        public int TasksCompleted { get; set; }
        public decimal MoneyEarned { get; set; }
    }
}
