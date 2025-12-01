using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Errando.Data;

public class TaskItem
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "Pending"; // ← ADD THIS (default: Pending)

    public bool IsCompleted { get; set; } = false;

    [Required]
    public int TaskId { get; set; }

    [JsonIgnore]
    public TodoTask? Task { get; set; }
}
