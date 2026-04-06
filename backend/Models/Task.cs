using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Errando.Data
{
    public class TodoTask
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime ScheduledTime { get; set; }

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Pending";

        [Required]
        public int ClientId { get; set; }

        [ForeignKey("ClientId")]
        public User? Client { get; set; }

        public int? RunnerId { get; set; }

        [ForeignKey("RunnerId")]
        public User? Runner { get; set; }

        // Location field - Lithuanian cities
        [MaxLength(100)]
        public string? Location { get; set; }

        // Price field
        [Column(TypeName = "decimal(18, 2)")]
        public decimal? Price { get; set; }

        // Periodicity fields
        public bool IsRecurring { get; set; } = false;

        // Day of week (0 = Sunday, 1 = Monday, ..., 6 = Saturday)
        public int? RecurringDayOfWeek { get; set; }

        // Number of repetitions
        public int? RecurringRepetitions { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property for related task items
        public ICollection<TaskItem> TaskItems { get; set; } = new List<TaskItem>();
    }
}
