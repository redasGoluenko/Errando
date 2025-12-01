using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Errando.Data;

public class StatusLog
{
    public int Id { get; set; }

    [Required]
    public int TaskItemId { get; set; }

    [JsonIgnore]
    public TaskItem? TaskItem { get; set; }

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = string.Empty;

    [StringLength(500)]
    public string Comment { get; set; } = string.Empty;

    [Required]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [Required]
    public int RunnerId { get; set; }

    public User? Runner { get; set; }
}
