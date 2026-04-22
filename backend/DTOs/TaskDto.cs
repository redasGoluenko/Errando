namespace Errando.DTOs;

public class CreateTaskDto
{
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime ScheduledTime { get; set; }
    public int ClientId { get; set; }
    public string? Location { get; set; }
    public decimal? Price { get; set; }
    public bool IsRecurring { get; set; } = false;
    public int? RecurringDayOfWeek { get; set; }
    public int? RecurringRepetitions { get; set; }
    public DateTime? ExpirationDate { get; set; }
}

public class UpdateTaskDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime ScheduledTime { get; set; }
    public int ClientId { get; set; }
    public string? Location { get; set; }
    public decimal? Price { get; set; }
    public bool IsRecurring { get; set; } = false;
    public int? RecurringDayOfWeek { get; set; }
    public int? RecurringRepetitions { get; set; }
    public DateTime? ExpirationDate { get; set; }
}

public class TaskDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime ScheduledTime { get; set; }
    public string Status { get; set; } = null!;
    public int ClientId { get; set; }
    public string? ClientUsername { get; set; }
    public int? RunnerId { get; set; }
    public string? RunnerUsername { get; set; }
    public string? Location { get; set; }
    public decimal? Price { get; set; }
    public bool IsRecurring { get; set; }
    public int? RecurringDayOfWeek { get; set; }
    public int? RecurringRepetitions { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public bool IsExpired { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsCompleted { get; set; }
}