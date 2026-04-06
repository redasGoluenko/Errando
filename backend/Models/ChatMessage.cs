using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Errando.Data
{
    public class ChatMessage
    {
        public int Id { get; set; }

        [Required]
        public int ChatId { get; set; }

        [ForeignKey("ChatId")]
        public Chat? Chat { get; set; }

        [Required]
        public int SenderId { get; set; }

        [ForeignKey("SenderId")]
        public User? Sender { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Content { get; set; } = string.Empty;

        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}
