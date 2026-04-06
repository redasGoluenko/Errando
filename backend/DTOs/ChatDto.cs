using System;
using System.Collections.Generic;

namespace Errando.DTOs
{
    public class ChatDto
    {
        public int Id { get; set; }
        public int User1Id { get; set; }
        public UserDto? User1 { get; set; }
        public int User2Id { get; set; }
        public UserDto? User2 { get; set; }
        public int? TaskId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<ChatMessageDto> Messages { get; set; } = new();
    }

    public class CreateChatDto
    {
        public int OtherUserId { get; set; }
        public int? TaskId { get; set; }
    }
}
