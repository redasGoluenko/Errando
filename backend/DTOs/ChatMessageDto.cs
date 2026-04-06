using System;

namespace Errando.DTOs
{
    public class ChatMessageDto
    {
        public int Id { get; set; }
        public int ChatId { get; set; }
        public int SenderId { get; set; }
        public string SenderUsername { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
    }

    public class SendChatMessageDto
    {
        public string Content { get; set; } = string.Empty;
    }
}
