using System.Text.Json.Serialization;

public class StatusLog
{
    public int Id { get; set; }

    public int TaskItemId { get; set; }
    [JsonIgnore]
    public TaskItem TaskItem { get; set; } = null!;

    public int? RunnerId { get; set; }
    public User? Runner { get; set; }

    public string Comment { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
