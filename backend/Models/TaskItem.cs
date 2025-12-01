using System.ComponentModel.DataAnnotations;

namespace Errando.Data;

public class TaskItem
{
    public int Id { get; set; }

    [Required]
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    public bool IsCompleted { get; set; } = false;

    [Required]
    public int TaskId { get; set; }

    // Navigation property
    public TodoTask? Task { get; set; } // ← CHANGED to TodoTask
}
