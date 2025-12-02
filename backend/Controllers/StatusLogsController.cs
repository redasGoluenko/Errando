using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Errando.Data;
using System.Security.Claims;

namespace backend.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class StatusLogsController : ControllerBase
{
    private readonly AppDbContext _context;

    public StatusLogsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetStatusLogs([FromQuery] int? taskItemId = null)
    {
        var query = _context.StatusLogs
            .Include(s => s.Runner)
            .AsQueryable();

        if (taskItemId.HasValue)
        {
            query = query.Where(s => s.TaskItemId == taskItemId.Value);
        }

        var logs = await query
            .OrderByDescending(s => s.Timestamp)
            .ToListAsync();

        return Ok(logs.Select(s => new
        {
            id = s.Id,
            taskItemId = s.TaskItemId,
            status = s.Status,
            comment = s.Comment,
            timestamp = s.Timestamp,
            runnerId = s.RunnerId,
            runner = s.Runner != null ? new
            {
                id = s.Runner.Id,
                username = s.Runner.Username,
                role = s.Runner.Role
            } : null
        }));
    }

    [HttpPost]
    [Authorize(Roles = "Runner,Admin")]
    public async Task<ActionResult<object>> CreateStatusLog(CreateStatusLogDto dto)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        var statusLog = new StatusLog
        {
            TaskItemId = dto.TaskItemId,
            Status = dto.Status,
            Comment = dto.Comment,
            RunnerId = userId,
            Timestamp = DateTime.UtcNow
        };

        _context.StatusLogs.Add(statusLog);
        await _context.SaveChangesAsync();

        var createdLog = await _context.StatusLogs
            .Include(s => s.Runner)
            .FirstOrDefaultAsync(s => s.Id == statusLog.Id);

        return CreatedAtAction(nameof(GetStatusLogs), new { id = statusLog.Id }, new
        {
            id = createdLog!.Id,
            taskItemId = createdLog.TaskItemId,
            status = createdLog.Status,
            comment = createdLog.Comment,
            timestamp = createdLog.Timestamp,
            runnerId = createdLog.RunnerId,
            runner = createdLog.Runner != null ? new
            {
                id = createdLog.Runner.Id,
                username = createdLog.Runner.Username,
                role = createdLog.Runner.Role
            } : null
        });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Runner,Admin")]
    public async Task<ActionResult<object>> UpdateStatusLog(int id, UpdateStatusLogDto dto)
    {
        var statusLog = await _context.StatusLogs
            .Include(s => s.Runner)
            .FirstOrDefaultAsync(s => s.Id == id);
        
        if (statusLog == null)
        {
            return NotFound(new { message = "Status log not found" });
        }

        // Check permissions: only owner or admin can update
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

        if (userRole != "Admin" && statusLog.RunnerId != userId)
        {
            return Forbid(); // 403 Forbidden if not owner or admin
        }

        // Update fields
        statusLog.Status = dto.Status;
        statusLog.Comment = dto.Comment;
        // Note: We keep the original Timestamp, don't update it

        await _context.SaveChangesAsync();

        return Ok(new
        {
            id = statusLog.Id,
            taskItemId = statusLog.TaskItemId,
            status = statusLog.Status,
            comment = statusLog.Comment,
            timestamp = statusLog.Timestamp,
            runnerId = statusLog.RunnerId,
            runner = statusLog.Runner != null ? new
            {
                id = statusLog.Runner.Id,
                username = statusLog.Runner.Username,
                role = statusLog.Runner.Role
            } : null
        });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Runner,Admin")]
    public async Task<IActionResult> DeleteStatusLog(int id)
    {
        var statusLog = await _context.StatusLogs.FindAsync(id);
        
        if (statusLog == null)
        {
            return NotFound(new { message = "Status log not found" });
        }

        // Optional: Only allow runner who created it or admin to delete
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

        if (userRole != "Admin" && statusLog.RunnerId != userId)
        {
            return Forbid(); // 403 Forbidden if not owner or admin
        }

        _context.StatusLogs.Remove(statusLog);
        await _context.SaveChangesAsync();

        return NoContent(); // 204 No Content
    }
}

public class CreateStatusLogDto
{
    public int TaskItemId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Comment { get; set; }
}

public class UpdateStatusLogDto
{
    public string Status { get; set; } = string.Empty;
    public string? Comment { get; set; }
}