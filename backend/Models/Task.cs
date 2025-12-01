using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Errando.Data;

public class TodoTask // ← RENAMED from Task
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public DateTime ScheduledTime { get; set; }

    public List<TaskItem> TaskItems { get; set; } = new();

    [Required]
    public int ClientId { get; set; }

    [JsonIgnore]
    public User? Client { get; set; }
}
