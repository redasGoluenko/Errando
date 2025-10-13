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

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTask(int id, UpdateTaskDto taskDto)
    {
        if (id != taskDto.Id) return BadRequest();

        var client = await _context.Users.FindAsync(taskDto.ClientId);
        if (client == null) return BadRequest("Client not found");

        var task = await _context.Tasks.FindAsync(id);
        if (task == null) return NotFound();

        task.Title = taskDto.Title;
        task.Description = taskDto.Description;
        task.ScheduledTime = taskDto.ScheduledTime;
        task.ClientId = taskDto.ClientId;

        _context.Entry(task).State = EntityState.Modified;
        
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
        var task = await _context.Tasks.FindAsync(taskId);
        if (task == null) return NotFound("Task not found");

        var taskItems = await _context.TaskItems
            .Where(ti => ti.TaskId == taskId && taskItemIds.Contains(ti.Id))
            .Include(ti => ti.StatusLogs)
            .ThenInclude(sl => sl.Runner)
            .ToListAsync();

        return taskItems;
    }

    [HttpPost("{taskId}/taskitems/filter")]
    public async Task<ActionResult<IEnumerable<TaskItem>>> GetTaskItemsByIdsPost(
        int taskId, 
        [FromBody] TaskItemFilterDto filterDto)
    {
        var task = await _context.Tasks.FindAsync(taskId);
        if (task == null) return NotFound("Task not found");

        var taskItems = await _context.TaskItems
            .Where(ti => ti.TaskId == taskId && filterDto.TaskItemIds.Contains(ti.Id))
            .Include(ti => ti.StatusLogs)
            .ThenInclude(sl => sl.Runner)
            .ToListAsync();

        return taskItems;
    }

    private bool TaskExists(int id)
    {
        return _context.Tasks.Any(e => e.Id == id);
    }
}