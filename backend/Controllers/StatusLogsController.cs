using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Errando.Data;

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
    public async Task<ActionResult<IEnumerable<StatusLog>>> GetStatusLogs()
        => await _context.StatusLogs
            .Include(sl => sl.TaskItem)
            .Include(sl => sl.Runner)
            .OrderByDescending(sl => sl.CreatedAt)
            .ToListAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<StatusLog>> GetStatusLog(int id)
    {
        var statusLog = await _context.StatusLogs
            .Include(sl => sl.TaskItem)
            .ThenInclude(ti => ti.Task)
            .Include(sl => sl.Runner)
            .FirstOrDefaultAsync(sl => sl.Id == id);
        
        if (statusLog == null) return NotFound();
        return statusLog;
    }

    [HttpPost]
    public async Task<ActionResult<StatusLog>> CreateStatusLog(CreateStatusLogDto statusLogDto)
    {
        // Verify that the TaskItemId exists
        var taskItem = await _context.TaskItems.FindAsync(statusLogDto.TaskItemId);
        if (taskItem == null) return BadRequest("TaskItem not found");

        // Verify that the RunnerId exists (if provided)
        if (statusLogDto.RunnerId.HasValue)
        {
            var runner = await _context.Users.FindAsync(statusLogDto.RunnerId.Value);
            if (runner == null) return BadRequest("Runner not found");
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
        
        // Return the status log with related data included
        var createdStatusLog = await _context.StatusLogs
            .Include(sl => sl.Runner)
            .FirstOrDefaultAsync(sl => sl.Id == statusLog.Id);
            
        return CreatedAtAction(nameof(GetStatusLog), new { id = statusLog.Id }, createdStatusLog);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateStatusLog(int id, UpdateStatusLogDto statusLogDto)
    {
        if (id != statusLogDto.Id) return BadRequest();
        
        // Verify that the TaskItemId exists
        var taskItem = await _context.TaskItems.FindAsync(statusLogDto.TaskItemId);
        if (taskItem == null) return BadRequest("TaskItem not found");

        // Verify that the RunnerId exists (if provided)
        if (statusLogDto.RunnerId.HasValue)
        {
            var runner = await _context.Users.FindAsync(statusLogDto.RunnerId.Value);
            if (runner == null) return BadRequest("Runner not found");
        }

        var statusLog = await _context.StatusLogs.FindAsync(id);
        if (statusLog == null) return NotFound();

        // Update properties (but keep original CreatedAt)
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

    [HttpDelete("{id}")]
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