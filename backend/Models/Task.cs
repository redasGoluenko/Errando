using System.Text.Json.Serialization;

public class Task
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime ScheduledTime { get; set; }

    public List<TaskItem> TaskItems { get; set; } = new();

    public int ClientId { get; set; } 
    [JsonIgnore]
    public User Client { get; set; } = null!; 
}
