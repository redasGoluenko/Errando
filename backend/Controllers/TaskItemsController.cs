using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Errando.Data;

[ApiController]
[Route("api/[controller]")]
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
    public async Task<ActionResult<TaskItem>> CreateTaskItem(CreateTaskItemDto taskItemDto)
    {
        var task = await _context.Tasks.FindAsync(taskItemDto.TaskId);
        if (task == null) return BadRequest("Task not found");

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

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTaskItem(int id, UpdateTaskItemDto taskItemDto)
    {
        if (id != taskItemDto.Id) return BadRequest();
        
        var task = await _context.Tasks.FindAsync(taskItemDto.TaskId);
        if (task == null) return BadRequest("Task not found");

        var taskItem = await _context.TaskItems.FindAsync(id);
        if (taskItem == null) return NotFound();

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
        
        _context.TaskItems.Remove(taskItem);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("by-task/{taskId}")]
    public async Task<ActionResult<IEnumerable<TaskItem>>> GetTaskItemsByTask(int taskId)
    {
        var taskItems = await _context.TaskItems
            .Where(ti => ti.TaskId == taskId)
            .Include(ti => ti.Task)
            .Include(ti => ti.StatusLogs)
            .ToListAsync();
            
        return taskItems;
    }

    [HttpPatch("{id}/complete")]
    public async Task<IActionResult> MarkTaskItemComplete(int id)
    {
        var taskItem = await _context.TaskItems.FindAsync(id);
        if (taskItem == null) return NotFound();

        taskItem.IsCompleted = true;
        await _context.SaveChangesAsync();
        
        return NoContent();
    }

    [HttpPatch("{id}/incomplete")]
    public async Task<IActionResult> MarkTaskItemIncomplete(int id)
    {
        var taskItem = await _context.TaskItems.FindAsync(id);
        if (taskItem == null) return NotFound();

        taskItem.IsCompleted = false;
        await _context.SaveChangesAsync();
        
        return NoContent();
    }

    [HttpGet("{taskItemId}/statuslogs")]
    public async Task<ActionResult<IEnumerable<StatusLog>>> GetStatusLogsByIds(
        int taskItemId, 
        [FromQuery] int[] statusLogIds)
    {
        var taskItem = await _context.TaskItems.FindAsync(taskItemId);
        if (taskItem == null) return NotFound("TaskItem not found");

        var statusLogs = await _context.StatusLogs
            .Where(sl => sl.TaskItemId == taskItemId && statusLogIds.Contains(sl.Id))
            .Include(sl => sl.Runner)
            .Include(sl => sl.TaskItem)
            .OrderByDescending(sl => sl.CreatedAt)
            .ToListAsync();

        return statusLogs;
    }

    [HttpPost("{taskItemId}/statuslogs/filter")]
    public async Task<ActionResult<IEnumerable<StatusLog>>> GetStatusLogsByIdsPost(
        int taskItemId, 
        [FromBody] StatusLogFilterDto filterDto)
    {
        var taskItem = await _context.TaskItems.FindAsync(taskItemId);
        if (taskItem == null) return NotFound("TaskItem not found");

        var statusLogs = await _context.StatusLogs
            .Where(sl => sl.TaskItemId == taskItemId && filterDto.StatusLogIds.Contains(sl.Id))
            .Include(sl => sl.Runner)
            .Include(sl => sl.TaskItem)
            .OrderByDescending(sl => sl.CreatedAt)
            .ToListAsync();

        return statusLogs;
    }

    private bool TaskItemExists(int id)
    {
        return _context.TaskItems.Any(e => e.Id == id);
    }
}