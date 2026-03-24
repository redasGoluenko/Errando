namespace Errando.DTOs
{
    public class ComplaintDto
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public int TaskId { get; set; }
        public string TaskTitle { get; set; } = string.Empty;
        public int ClientId { get; set; }
        public string ClientUsername { get; set; } = string.Empty;
        public int RunnerId { get; set; }
        public string RunnerUsername { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsResolved { get; set; }
    }

    public class CreateComplaintDto
    {
        public string Description { get; set; } = string.Empty;
        public int TaskId { get; set; }
    }
}
