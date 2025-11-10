using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Errando.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/[controller]")]
[Authorize] // require authenticated user by default
public class StatusLogsController : ControllerBase
{
    private readonly AppDbContext _context;

    public StatusLogsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<StatusLog>>> GetStatusLogs()
    {
        if (User.IsInRole("Admin"))
        {
            return await _context.StatusLogs
                .Include(sl => sl.TaskItem)
                .Include(sl => sl.Runner)
                .OrderByDescending(sl => sl.CreatedAt)
                .ToListAsync();
        }

        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(claim, out var currentUserId)) return Forbid();

        if (User.IsInRole("Client"))
        {
            return await _context.StatusLogs
                .Include(sl => sl.TaskItem)
                .Include(sl => sl.Runner)
                .Where(sl => sl.TaskItem.Task.ClientId == currentUserId)
                .OrderByDescending(sl => sl.CreatedAt)
                .ToListAsync();
        }

        if (User.IsInRole("Runner"))
        {
            return await _context.StatusLogs
                .Include(sl => sl.TaskItem)
                .Include(sl => sl.Runner)
                .Where(sl => sl.RunnerId == currentUserId)
                .OrderByDescending(sl => sl.CreatedAt)
                .ToListAsync();
        }

        return Forbid();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<StatusLog>> GetStatusLog(int id)
    {
        var statusLog = await _context.StatusLogs
            .Include(sl => sl.TaskItem)
            .ThenInclude(ti => ti.Task)
            .Include(sl => sl.Runner)
            .FirstOrDefaultAsync(sl => sl.Id == id);

        if (statusLog == null) return NotFound();
        if (User.IsInRole("Admin")) return statusLog;

        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(claim, out var currentUserId)) return Forbid();

        if (User.IsInRole("Client") && statusLog.TaskItem.Task.ClientId == currentUserId) return statusLog;
        if (User.IsInRole("Runner") && statusLog.RunnerId == currentUserId) return statusLog;

        return Forbid();
    }

    // only Runner or Admin can create status logs
    [HttpPost]
    [Authorize(Policy = "RunnerOrAdmin")]
    public async Task<ActionResult<StatusLog>> CreateStatusLog(CreateStatusLogDto statusLogDto)
    {
        var taskItem = await _context.TaskItems.FindAsync(statusLogDto.TaskItemId);
        if (taskItem == null) return BadRequest("TaskItem not found");

        if (statusLogDto.RunnerId.HasValue)
        {
            var runner = await _context.Users.FindAsync(statusLogDto.RunnerId.Value);
            if (runner == null) return BadRequest("Runner not found");
        }

        // If not admin, ensure runnerId (if present) equals current user
        var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!User.IsInRole("Admin") && currentUserIdClaim != null)
        {
            var currentUserId = int.Parse(currentUserIdClaim);
            if (statusLogDto.RunnerId.HasValue && statusLogDto.RunnerId.Value != currentUserId)
                return Forbid();
            // if RunnerId not provided, assign current user as runner
            if (!statusLogDto.RunnerId.HasValue)
                statusLogDto.RunnerId = currentUserId;
        }

        var statusLog = new StatusLog
        {
            TaskItemId = statusLogDto.TaskItemId,
            RunnerId = statusLogDto.RunnerId,
            Comment = statusLogDto.Comment,
            CreatedAt = DateTime.UtcNow
        };

        _context.StatusLogs.Add(statusLog);
        await _context.SaveChangesAsync();

        var createdStatusLog = await _context.StatusLogs
            .Include(sl => sl.Runner)
            .FirstOrDefaultAsync(sl => sl.Id == statusLog.Id);
            
        return CreatedAtAction(nameof(GetStatusLog), new { id = statusLog.Id }, createdStatusLog);
    }

    // only Admin can update
    [HttpPatch("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> UpdateStatusLog(int id, UpdateStatusLogDto statusLogDto)
    {
        if (id != statusLogDto.Id) return BadRequest();
        
        var statusLog = await _context.StatusLogs.FindAsync(id);
        if (statusLog == null) return NotFound();

        if (statusLogDto.TaskItemId != statusLog.TaskItemId)
        {
            var taskItem = await _context.TaskItems.FindAsync(statusLogDto.TaskItemId);
            if (taskItem == null) return BadRequest("TaskItem not found");
        }

        if (statusLogDto.RunnerId != statusLog.RunnerId && statusLogDto.RunnerId.HasValue)
        {
            var runner = await _context.Users.FindAsync(statusLogDto.RunnerId.Value);
            if (runner == null) return BadRequest("Runner not found");
        }

        statusLog.TaskItemId = statusLogDto.TaskItemId;
        statusLog.RunnerId = statusLogDto.RunnerId;
        statusLog.Comment = statusLogDto.Comment;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!StatusLogExists(id))
                return NotFound();
            throw;
        }

        return NoContent();
    }

    // only Admin can delete
    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteStatusLog(int id)
    {
        var statusLog = await _context.StatusLogs.FindAsync(id);
        if (statusLog == null) return NotFound();
        
        _context.StatusLogs.Remove(statusLog);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("by-taskitem/{taskItemId}")]
    public async Task<ActionResult<IEnumerable<StatusLog>>> GetStatusLogsByTaskItem(int taskItemId)
    {
        var taskItem = await _context.TaskItems
            .Include(ti => ti.Task)
            .FirstOrDefaultAsync(ti => ti.Id == taskItemId);
        if (taskItem == null) return NotFound();

        if (User.IsInRole("Admin")) { /* continue */ }
        else
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(claim, out var currentUserId)) return Forbid();

            if (User.IsInRole("Client") && taskItem.Task.ClientId != currentUserId) return Forbid();
            if (User.IsInRole("Runner"))
            {
                // runner can view only if they have logs for this taskItem
                var has = await _context.StatusLogs.AnyAsync(sl => sl.TaskItemId == taskItemId && sl.RunnerId == currentUserId);
                if (!has) return Forbid();
            }
        }

        var statusLogs = await _context.StatusLogs
            .Where(sl => sl.TaskItemId == taskItemId)
            .Include(sl => sl.TaskItem)
            .Include(sl => sl.Runner)
            .OrderByDescending(sl => sl.CreatedAt)
            .ToListAsync();

        return statusLogs;
    }

    [HttpGet("by-runner/{runnerId}")]
    public async Task<ActionResult<IEnumerable<StatusLog>>> GetStatusLogsByRunner(int runnerId)
    {
        if (User.IsInRole("Admin")) { /* allow */ }
        else
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(claim, out var currentUserId)) return Forbid();

            // only the runner themself can query their logs
            if (!User.IsInRole("Runner") || currentUserId != runnerId) return Forbid();
        }

        var statusLogs = await _context.StatusLogs
            .Where(sl => sl.RunnerId == runnerId)
            .Include(sl => sl.TaskItem)
            .ThenInclude(ti => ti.Task)
            .Include(sl => sl.Runner)
            .OrderByDescending(sl => sl.CreatedAt)
            .ToListAsync();
            
        return statusLogs;
    }

    private bool StatusLogExists(int id)
    {
        return _context.StatusLogs.Any(e => e.Id == id);
    }
}