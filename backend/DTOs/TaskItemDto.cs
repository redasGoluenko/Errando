public class CreateTaskItemDto
{
    public string Description { get; set; } = null!;
    public bool IsCompleted { get; set; } = false;
    public int TaskId { get; set; }
}

public class UpdateTaskItemDto
{
    public int Id { get; set; }
    public string Description { get; set; } = null!;
    public bool IsCompleted { get; set; }
    public int TaskId { get; set; }
}