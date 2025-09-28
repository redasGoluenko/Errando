public class TaskItem
{
    public int Id { get; set; }
    public string Description { get; set; } = null!;
    public bool IsCompleted { get; set; } = false;

    public int TaskId { get; set; }
    public Task Task { get; set; } = null!;

    public List<StatusLog> StatusLogs { get; set; } = new();
}
