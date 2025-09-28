public class StatusLog
{
    public int Id { get; set; }
    public string Status { get; set; } = null!;
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public string? Comment { get; set; }

    public int TaskItemId { get; set; }
    public TaskItem TaskItem { get; set; } = null!;
}
