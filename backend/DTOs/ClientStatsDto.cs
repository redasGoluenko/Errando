namespace Errando.DTOs
{
    public class ClientStatsDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public decimal Rating { get; set; }
        public int TasksCompleted { get; set; }
    }
}
