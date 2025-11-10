using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Errando.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TaskItemsController : ControllerBase
{
    private readonly AppDbContext _context;

    public TaskItemsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskItem>>> GetTaskItems()
        => await _context.TaskItems
            .Include(ti => ti.Task)
            .Include(ti => ti.StatusLogs)
            .ToListAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<TaskItem>> GetTaskItem(int id)
    {
        var taskItem = await _context.TaskItems
            .Include(ti => ti.Task)
            .Include(ti => ti.StatusLogs)
            .ThenInclude(sl => sl.Runner)
            .FirstOrDefaultAsync(ti => ti.Id == id);

        if (taskItem == null) return NotFound();
        return taskItem;
    }

    [HttpPost]
    [Authorize(Policy = "ClientOrAdmin")]
    public async Task<ActionResult<TaskItem>> CreateTaskItem(CreateTaskItemDto taskItemDto)
    {
        var task = await _context.Tasks.FindAsync(taskItemDto.TaskId);
        if (task == null) return BadRequest("Task not found");

        // only Admin or the client who owns the parent task can create
        if (!User.IsInRole("Admin"))
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId == null || currentUserId != task.ClientId.ToString())
                return Forbid();
        }

        var taskItem = new TaskItem
        {
            Description = taskItemDto.Description,
            IsCompleted = taskItemDto.IsCompleted,
            TaskId = taskItemDto.TaskId
        };

        _context.TaskItems.Add(taskItem);
        await _context.SaveChangesAsync();

        var createdTaskItem = await _context.TaskItems
            .Include(ti => ti.Task)
            .FirstOrDefaultAsync(ti => ti.Id == taskItem.Id);

        return CreatedAtAction(nameof(GetTaskItem), new { id = taskItem.Id }, createdTaskItem);
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateTaskItem(int id, UpdateTaskItemDto taskItemDto)
    {
        if (id != taskItemDto.Id) return BadRequest();

        var taskItem = await _context.TaskItems.FindAsync(id);
        if (taskItem == null) return NotFound();

        var parentTask = await _context.Tasks.FindAsync(taskItem.TaskId);
        if (parentTask == null) return BadRequest("Parent task not found");

        // only Admin or parent task owner can update
        if (!User.IsInRole("Admin"))
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId == null || currentUserId != parentTask.ClientId.ToString())
                return Forbid();
        }

        if (taskItemDto.TaskId != taskItem.TaskId)
        {
            var task = await _context.Tasks.FindAsync(taskItemDto.TaskId);
            if (task == null) return BadRequest("Task not found");
        }

        taskItem.Description = taskItemDto.Description;
        taskItem.IsCompleted = taskItemDto.IsCompleted;
        taskItem.TaskId = taskItemDto.TaskId;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!TaskItemExists(id))
                return NotFound();
            throw;
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTaskItem(int id)
    {
        var taskItem = await _context.TaskItems.FindAsync(id);
        if (taskItem == null) return NotFound();

        var parentTask = await _context.Tasks.FindAsync(taskItem.TaskId);
        if (parentTask == null) return BadRequest("Parent task not found");

        if (!User.IsInRole("Admin"))
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId == null || currentUserId != parentTask.ClientId.ToString())
                return Forbid();
        }

        _context.TaskItems.Remove(taskItem);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPatch("{id}/complete")]
    public async Task<IActionResult> MarkTaskItemComplete(int id)
    {
        var taskItem = await _context.TaskItems.FindAsync(id);
        if (taskItem == null) return NotFound();

        var parentTask = await _context.Tasks.FindAsync(taskItem.TaskId);
        if (parentTask == null) return BadRequest("Parent task not found");

        if (!User.IsInRole("Admin"))
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId == null || currentUserId != parentTask.ClientId.ToString())
                return Forbid();
        }

        taskItem.IsCompleted = true;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPatch("{id}/incomplete")]
    public async Task<IActionResult> MarkTaskItemIncomplete(int id)
    {
        var taskItem = await _context.TaskItems.FindAsync(id);
        if (taskItem == null) return NotFound();

        var parentTask = await _context.Tasks.FindAsync(taskItem.TaskId);
        if (parentTask == null) return BadRequest("Parent task not found");

        if (!User.IsInRole("Admin"))
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId == null || currentUserId != parentTask.ClientId.ToString())
                return Forbid();
        }

        taskItem.IsCompleted = false;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool TaskItemExists(int id)
    {
        return _context.TaskItems.Any(e => e.Id == id);
    }
}