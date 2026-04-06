using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Errando.Data;
using Errando.DTOs;
using System.Security.Claims;

namespace backend.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ChatsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ChatsController(AppDbContext context)
    {
        _context = context;
    }

    private int GetCurrentUserId()
    {
        return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
    }

    private string GetCurrentUserRole()
    {
        return User.FindFirst(ClaimTypes.Role)?.Value ?? "";
    }

    /// <summary>
    /// Get all chats for the current user
    /// Shows all conversations the user is part of, with the last message
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ChatDto>>> GetChats()
    {
        var currentUserId = GetCurrentUserId();
        var userRole = GetCurrentUserRole();

        // Get all chats where the user is one of the participants
        var chats = await _context.Chats
            .Where(c => c.User1Id == currentUserId || c.User2Id == currentUserId)
            .Include(c => c.User1)
            .Include(c => c.User2)
            .Include(c => c.Messages)
            .ThenInclude(m => m.Sender)
            .OrderByDescending(c => c.UpdatedAt)
            .ToListAsync();

        var chatDtos = chats.Select(c => new ChatDto
        {
            Id = c.Id,
            User1Id = c.User1Id,
            User1 = c.User1 != null ? new UserDto
            {
                Id = c.User1.Id,
                Username = c.User1.Username,
                Email = c.User1.Email,
                Role = c.User1.Role
            } : null,
            User2Id = c.User2Id,
            User2 = c.User2 != null ? new UserDto
            {
                Id = c.User2.Id,
                Username = c.User2.Username,
                Email = c.User2.Email,
                Role = c.User2.Role
            } : null,
            TaskId = c.TaskId,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt,
            Messages = c.Messages
                .OrderByDescending(m => m.SentAt)
                .Take(1)
                .Select(m => new ChatMessageDto
                {
                    Id = m.Id,
                    ChatId = m.ChatId,
                    SenderId = m.SenderId,
                    SenderUsername = m.Sender?.Username ?? "",
                    Content = m.Content,
                    SentAt = m.SentAt
                })
                .ToList()
        }).ToList();

        return Ok(chatDtos);
    }

    /// <summary>
    /// Get all messages in a specific chat
    /// </summary>
    [HttpGet("{chatId}")]
    public async Task<ActionResult<ChatDto>> GetChat(int chatId)
    {
        var currentUserId = GetCurrentUserId();

        var chat = await _context.Chats
            .Where(c => c.Id == chatId && (c.User1Id == currentUserId || c.User2Id == currentUserId))
            .Include(c => c.User1)
            .Include(c => c.User2)
            .Include(c => c.Messages)
            .ThenInclude(m => m.Sender)
            .FirstOrDefaultAsync();

        if (chat == null)
            return NotFound(new { message = "Chat not found" });

        var chatDto = new ChatDto
        {
            Id = chat.Id,
            User1Id = chat.User1Id,
            User1 = chat.User1 != null ? new UserDto
            {
                Id = chat.User1.Id,
                Username = chat.User1.Username,
                Email = chat.User1.Email,
                Role = chat.User1.Role
            } : null,
            User2Id = chat.User2Id,
            User2 = chat.User2 != null ? new UserDto
            {
                Id = chat.User2.Id,
                Username = chat.User2.Username,
                Email = chat.User2.Email,
                Role = chat.User2.Role
            } : null,
            TaskId = chat.TaskId,
            CreatedAt = chat.CreatedAt,
            UpdatedAt = chat.UpdatedAt,
            Messages = chat.Messages
                .OrderBy(m => m.SentAt)
                .Select(m => new ChatMessageDto
                {
                    Id = m.Id,
                    ChatId = m.ChatId,
                    SenderId = m.SenderId,
                    SenderUsername = m.Sender?.Username ?? "",
                    Content = m.Content,
                    SentAt = m.SentAt
                })
                .ToList()
        };

        return Ok(chatDto);
    }

    /// <summary>
    /// Create a new chat or get an existing one with another user
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ChatDto>> CreateOrGetChat([FromBody] CreateChatDto createChatDto)
    {
        var currentUserId = GetCurrentUserId();
        var otherUserId = createChatDto.OtherUserId;

        if (currentUserId == otherUserId)
            return BadRequest(new { message = "Cannot chat with yourself" });

        // Check if users exist
        var otherUser = await _context.Users.FindAsync(otherUserId);
        if (otherUser == null)
            return BadRequest(new { message = "Other user not found" });

        // Check if chat already exists (order doesn't matter for 1-on-1 chat)
        var existingChat = await _context.Chats
            .Where(c =>
                (c.User1Id == currentUserId && c.User2Id == otherUserId) ||
                (c.User1Id == otherUserId && c.User2Id == currentUserId)
            )
            .Include(c => c.User1)
            .Include(c => c.User2)
            .Include(c => c.Messages)
            .ThenInclude(m => m.Sender)
            .FirstOrDefaultAsync();

        if (existingChat != null)
        {
            return Ok(MapChatToDto(existingChat));
        }

        // Canonicalize: always put lower ID as User1, higher ID as User2
        var user1Id = Math.Min(currentUserId, otherUserId);
        var user2Id = Math.Max(currentUserId, otherUserId);

        // Create new chat
        var newChat = new Chat
        {
            User1Id = user1Id,
            User2Id = user2Id,
            TaskId = createChatDto.TaskId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Chats.Add(newChat);
        await _context.SaveChangesAsync();

        // Reload the chat with all related entities
        var createdChat = await _context.Chats
            .Where(c => c.Id == newChat.Id)
            .Include(c => c.User1)
            .Include(c => c.User2)
            .Include(c => c.Messages)
            .ThenInclude(m => m.Sender)
            .FirstOrDefaultAsync();

        return CreatedAtAction(nameof(GetChat), new { chatId = newChat.Id }, MapChatToDto(createdChat!));
    }

    /// <summary>
    /// Send a message in a chat
    /// </summary>
    [HttpPost("{chatId}/messages")]
    public async Task<ActionResult<ChatMessageDto>> SendMessage(int chatId, [FromBody] SendChatMessageDto sendChatMessageDto)
    {
        var currentUserId = GetCurrentUserId();

        // Verify the user is part of this chat
        var chat = await _context.Chats
            .Where(c => c.Id == chatId && (c.User1Id == currentUserId || c.User2Id == currentUserId))
            .FirstOrDefaultAsync();

        if (chat == null)
            return NotFound(new { message = "Chat not found" });

        if (string.IsNullOrWhiteSpace(sendChatMessageDto.Content))
            return BadRequest(new { message = "Message content cannot be empty" });

        var message = new ChatMessage
        {
            ChatId = chatId,
            SenderId = currentUserId,
            Content = sendChatMessageDto.Content
        };

        _context.ChatMessages.Add(message);

        // Update chat's UpdatedAt timestamp
        chat.UpdatedAt = DateTime.UtcNow;
        _context.Chats.Update(chat);

        await _context.SaveChangesAsync();

        // Reload message with sender information
        await _context.Entry(message)
            .Reference(m => m.Sender).LoadAsync();

        var messageDto = new ChatMessageDto
        {
            Id = message.Id,
            ChatId = message.ChatId,
            SenderId = message.SenderId,
            SenderUsername = message.Sender?.Username ?? "",
            Content = message.Content,
            SentAt = message.SentAt
        };

        return CreatedAtAction(nameof(SendMessage), new { chatId = chatId }, messageDto);
    }

    /// <summary>
    /// Get chat participants (users relevant to the current user based on task assignments)
    /// Returns potential chat participants based on:
    /// - For Clients: Runners assigned to their tasks
    /// - For Runners: Clients whose tasks are assigned to them
    /// </summary>
    [HttpGet("participants")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetChatParticipants()
    {
        var currentUserId = GetCurrentUserId();
        var userRole = GetCurrentUserRole();

        HashSet<int> participantIds = new();

        if (userRole == "Client")
        {
            // Get all runners assigned to the client's tasks
            var runnerIds = await _context.Tasks
                .Where(t => t.ClientId == currentUserId && t.RunnerId.HasValue)
                .Select(t => t.RunnerId.Value)
                .Distinct()
                .ToListAsync();

            participantIds = new HashSet<int>(runnerIds);
        }
        else if (userRole == "Runner")
        {
            // Get all clients whose tasks are assigned to this runner
            var clientIds = await _context.Tasks
                .Where(t => t.RunnerId == currentUserId)
                .Select(t => t.ClientId)
                .Distinct()
                .ToListAsync();

            participantIds = new HashSet<int>(clientIds);
        }
        // Admin can see all users

        var participants = await _context.Users
            .Where(u => u.Id != currentUserId && (participantIds.Count == 0 || participantIds.Contains(u.Id)))
            .Select(u => new UserDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                Role = u.Role
            })
            .ToListAsync();

        return Ok(participants);
    }

    /// <summary>
    /// Delete a chat
    /// </summary>
    [HttpDelete("{chatId}")]
    public async Task<IActionResult> DeleteChat(int chatId)
    {
        var currentUserId = GetCurrentUserId();

        var chat = await _context.Chats
            .Where(c => c.Id == chatId && (c.User1Id == currentUserId || c.User2Id == currentUserId))
            .FirstOrDefaultAsync();

        if (chat == null)
            return NotFound(new { message = "Chat not found" });

        // Delete all messages first
        var messages = await _context.ChatMessages.Where(m => m.ChatId == chatId).ToListAsync();
        _context.ChatMessages.RemoveRange(messages);

        // Delete the chat
        _context.Chats.Remove(chat);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private ChatDto MapChatToDto(Chat chat)
    {
        return new ChatDto
        {
            Id = chat.Id,
            User1Id = chat.User1Id,
            User1 = chat.User1 != null ? new UserDto
            {
                Id = chat.User1.Id,
                Username = chat.User1.Username,
                Email = chat.User1.Email,
                Role = chat.User1.Role
            } : null,
            User2Id = chat.User2Id,
            User2 = chat.User2 != null ? new UserDto
            {
                Id = chat.User2.Id,
                Username = chat.User2.Username,
                Email = chat.User2.Email,
                Role = chat.User2.Role
            } : null,
            TaskId = chat.TaskId,
            CreatedAt = chat.CreatedAt,
            UpdatedAt = chat.UpdatedAt,
            Messages = chat.Messages
                .OrderBy(m => m.SentAt)
                .Select(m => new ChatMessageDto
                {
                    Id = m.Id,
                    ChatId = m.ChatId,
                    SenderId = m.SenderId,
                    SenderUsername = m.Sender?.Username ?? "",
                    Content = m.Content,
                    SentAt = m.SentAt
                })
                .ToList()
        };
    }
}
