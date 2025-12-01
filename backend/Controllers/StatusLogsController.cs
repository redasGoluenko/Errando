using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Errando.Data;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StatusLogsController : ControllerBase
{
    private readonly AppDbContext _context;

    public StatusLogsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/StatusLogs?taskItemId={id}
    [HttpGet]
    public async Task<ActionResult<IEnumerable<StatusLog>>> GetStatusLogs([FromQuery] int taskItemId)
    {
        Console.WriteLine($"🔍 GET STATUS LOGS FOR TASK ITEM: {taskItemId}");
        
        var logs = await _context.StatusLogs
            .Include(sl => sl.Runner)
            .Where(sl => sl.TaskItemId == taskItemId)
            .OrderByDescending(sl => sl.Timestamp)
            .ToListAsync();

        Console.WriteLine($"✅ FOUND {logs.Count} STATUS LOGS");
        return logs;
    }

    // POST: api/StatusLogs
    [HttpPost]
    [Authorize(Roles = "Runner,Admin")]
    public async Task<ActionResult<StatusLog>> CreateStatusLog(CreateStatusLogRequest request)
    {
        Console.WriteLine($"📤 CREATE STATUS LOG: TaskItemId={request.TaskItemId}, Status={request.Status}");

        var taskItem = await _context.TaskItems
            .Include(ti => ti.Task)
            .FirstOrDefaultAsync(ti => ti.Id == request.TaskItemId);

        if (taskItem == null)
        {
            return NotFound(new { message = "Task item not found" });
        }

        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

        // Check if runner is assigned to this task
        if (userRole == "Runner" && taskItem.Task?.RunnerId != userId)
        {
            return Forbid();
        }

        // Update task item status
        taskItem.Status = request.Status;

        // Create status log
        var statusLog = new StatusLog
        {
            TaskItemId = request.TaskItemId,
            Status = request.Status,
            Comment = request.Comment ?? string.Empty,
            Timestamp = DateTime.UtcNow,
            RunnerId = userId
        };

        _context.StatusLogs.Add(statusLog);
        await _context.SaveChangesAsync();

        // Reload with Runner data
        var savedLog = await _context.StatusLogs
            .Include(sl => sl.Runner)
            .FirstOrDefaultAsync(sl => sl.Id == statusLog.Id);

        Console.WriteLine($"✅ STATUS LOG CREATED: {savedLog?.Id}");
        return CreatedAtAction(nameof(GetStatusLogs), new { taskItemId = request.TaskItemId }, savedLog);
    }
}

public class CreateStatusLogRequest
{
    public int TaskItemId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Comment { get; set; }
}