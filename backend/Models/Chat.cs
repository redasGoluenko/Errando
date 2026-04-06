using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Errando.Data
{
    public class Chat
    {
        public int Id { get; set; }

        [Required]
        public int User1Id { get; set; }

        [ForeignKey("User1Id")]
        public User? User1 { get; set; }

        [Required]
        public int User2Id { get; set; }

        [ForeignKey("User2Id")]
        public User? User2 { get; set; }

        // Optional: The task that initiated this chat
        public int? TaskId { get; set; }

        [ForeignKey("TaskId")]
        public TodoTask? Task { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property for chat messages
        public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }
}
