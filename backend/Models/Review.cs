using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Errando.Data
{
    public class Review
    {
        public int Id { get; set; }

        [Required]
        public int TaskId { get; set; }

        [ForeignKey("TaskId")]
        public TodoTask? Task { get; set; }

        // Person leaving the review
        [Required]
        public int ReviewerId { get; set; }

        [ForeignKey("ReviewerId")]
        public User? Reviewer { get; set; }

        // Person being reviewed
        [Required]
        public int RevieweeId { get; set; }

        [ForeignKey("RevieweeId")]
        public User? Reviewee { get; set; }

        // Review details
        [Required]
        [Range(1, 5)]
        public int StarRating { get; set; }

        [MaxLength(1000)]
        public string? ReviewText { get; set; }

        // Calculated rating (taking into account keywords, speed, etc.)
        [Column(TypeName = "decimal(3, 2)")]
        public decimal FinalRating { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
