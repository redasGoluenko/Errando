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

    public int? RunnerId { get; set; }

    [JsonIgnore]
    public User? Runner { get; set; }

    [Required]
    [StringLength(500)]
    public string Comment { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
