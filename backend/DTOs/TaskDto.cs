public class CreateTaskDto
{
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime ScheduledTime { get; set; }
    public int ClientId { get; set; }
}

public class UpdateTaskDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime ScheduledTime { get; set; }
    public int ClientId { get; set; }
}