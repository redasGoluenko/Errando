using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Errando.Data;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly AppDbContext _context;

    public TasksController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Task>>> GetTasks()
        => await _context.Tasks
            .Include(t => t.Client)
            .Include(t => t.TaskItems)
            .ThenInclude(ti => ti.StatusLogs)
            .ThenInclude(sl => sl.Runner)  // Optional: include runner info in status logs
            .ToListAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<Task>> GetTask(int id)
    {
        var task = await _context.Tasks
            .Include(t => t.Client)
            .Include(t => t.TaskItems)
            .ThenInclude(ti => ti.StatusLogs)
            .FirstOrDefaultAsync(t => t.Id == id);
        
        if (task == null) return NotFound();
        return task;
    }

    [HttpPost]
    public async Task<ActionResult<Task>> CreateTask(CreateTaskDto taskDto)
    {
        var client = await _context.Users.FindAsync(taskDto.ClientId);
        if (client == null) return BadRequest("Client not found");

        var task = new Task
        {
            Title = taskDto.Title,
            Description = taskDto.Description,
            ScheduledTime = taskDto.ScheduledTime,
            ClientId = taskDto.ClientId
        };

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();
   
        var createdTask = await _context.Tasks
            .Include(t => t.Client)
            .FirstOrDefaultAsync(t => t.Id == task.Id);
            
        return CreatedAtAction(nameof(GetTask), new { id = task.Id }, createdTask);
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateTask(int id, UpdateTaskDto taskDto)
    {
        if (id != taskDto.Id) return BadRequest();

        var task = await _context.Tasks.FindAsync(id);
        if (task == null) return NotFound();

        if (taskDto.ClientId != task.ClientId)
        {
            var client = await _context.Users.FindAsync(taskDto.ClientId);
            if (client == null) return BadRequest("Client not found");
        }

        task.Title = taskDto.Title;
        task.Description = taskDto.Description;
        task.ScheduledTime = taskDto.ScheduledTime;
        task.ClientId = taskDto.ClientId;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!TaskExists(id))
                return NotFound();
            throw;
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task == null) return NotFound();
        
        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("by-client/{clientId}")]
    public async Task<ActionResult<IEnumerable<Task>>> GetTasksByClient(int clientId)
    {
        var tasks = await _context.Tasks
            .Where(t => t.ClientId == clientId)
            .Include(t => t.Client)
            .Include(t => t.TaskItems)
            .ToListAsync();
            
        return tasks;
    }

    [HttpGet("{taskId}/taskitems")]
    public async Task<ActionResult<IEnumerable<TaskItem>>> GetTaskItemsByIds(
        int taskId, 
        [FromQuery] int[] taskItemIds)
    {
        // Check if task exists
        var task = await _context.Tasks.FindAsync(taskId);
        if (task == null) return NotFound("Task not found");

        IQueryable<TaskItem> query = _context.TaskItems
            .Where(ti => ti.TaskId == taskId);

        if (taskItemIds != null && taskItemIds.Length > 0)
        {
            // Check if specified task item IDs exist in this task
            var existingIds = await _context.TaskItems
                .Where(ti => ti.TaskId == taskId && taskItemIds.Contains(ti.Id))
                .Select(ti => ti.Id)
                .ToListAsync();

            var missingIds = taskItemIds.Except(existingIds).ToArray();
            if (missingIds.Length > 0)
            {
                return NotFound($"TaskItem(s) not found: {string.Join(", ", missingIds)}");
            }

            query = query.Where(ti => taskItemIds.Contains(ti.Id));
        }

        var taskItems = await query
            .Include(ti => ti.StatusLogs)
            .ThenInclude(sl => sl.Runner)
            .ToListAsync();

        return taskItems;
    }

    // NEW: Get specific task item within a task
    [HttpGet("{taskId}/taskitems/{taskItemId}")]
    public async Task<ActionResult<TaskItem>> GetTaskItem(int taskId, int taskItemId)
    {
        var task = await _context.Tasks.FindAsync(taskId);
        if (task == null) return NotFound("Task not found");

        var taskItem = await _context.TaskItems
            .Where(ti => ti.TaskId == taskId && ti.Id == taskItemId)
            .Include(ti => ti.StatusLogs)
            .ThenInclude(sl => sl.Runner)
            .FirstOrDefaultAsync();

        if (taskItem == null) return NotFound("TaskItem not found");
        return taskItem;
    }

    // NEW: Get all status logs for a specific task item within a task
    [HttpGet("{taskId}/taskitems/{taskItemId}/statuslogs")]
    public async Task<ActionResult<IEnumerable<StatusLog>>> GetStatusLogsForTaskItem(
        int taskId, 
        int taskItemId,
        [FromQuery] int[] statusLogIds)
    {
        // Check if task exists
        var task = await _context.Tasks.FindAsync(taskId);
        if (task == null) return NotFound("Task not found");

        // Check if task item exists and belongs to the task
        var taskItem = await _context.TaskItems
            .Where(ti => ti.TaskId == taskId && ti.Id == taskItemId)
            .FirstOrDefaultAsync();
        if (taskItem == null) return NotFound("TaskItem not found or does not belong to specified task");

        IQueryable<StatusLog> query = _context.StatusLogs
            .Where(sl => sl.TaskItemId == taskItemId);

        if (statusLogIds != null && statusLogIds.Length > 0)
        {
            // Check if specified status log IDs exist for this task item
            var existingIds = await _context.StatusLogs
                .Where(sl => sl.TaskItemId == taskItemId && statusLogIds.Contains(sl.Id))
                .Select(sl => sl.Id)
                .ToListAsync();

            var missingIds = statusLogIds.Except(existingIds).ToArray();
            if (missingIds.Length > 0)
            {
                return NotFound($"StatusLog(s) not found: {string.Join(", ", missingIds)}");
            }

            query = query.Where(sl => statusLogIds.Contains(sl.Id));
        }

        var statusLogs = await query
            .Include(sl => sl.Runner)
            .Include(sl => sl.TaskItem)
            .OrderByDescending(sl => sl.CreatedAt)
            .ToListAsync();

        return statusLogs;
    }

    private bool TaskExists(int id)
    {
        return _context.Tasks.Any(e => e.Id == id);
    }
}