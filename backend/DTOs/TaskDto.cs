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
}