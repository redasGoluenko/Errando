using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Errando.Data; // ← CHANGE FROM backend TO Errando.Data

namespace backend.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TaskItemsController : ControllerBase
{
    private readonly AppDbContext _context;

    public TaskItemsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/TaskItems?taskId=1
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskItem>>> GetTaskItems([FromQuery] int? taskId)
    {
        // If taskId is provided, filter by it
        if (taskId.HasValue)
        {
            return await _context.TaskItems
                .Where(ti => ti.TaskId == taskId.Value)
                .OrderBy(ti => ti.Id)
                .ToListAsync();
        }

        // Otherwise return all (for admin/debugging)
        return await _context.TaskItems
            .OrderBy(ti => ti.TaskId)
            .ThenBy(ti => ti.Id)
            .ToListAsync();
    }

    // GET: api/TaskItems/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<TaskItem>> GetTaskItem(int id)
    {
        var taskItem = await _context.TaskItems.FindAsync(id);

        if (taskItem == null)
        {
            return NotFound(new { message = "Task item not found" });
        }

        return taskItem;
    }

    // POST: api/TaskItems
    [HttpPost]
    public async Task<ActionResult<TaskItem>> CreateTaskItem([FromBody] TaskItem taskItem)
    {
        // DEBUG: Log incoming data
        Console.WriteLine($"📥 CREATE TaskItem - TaskId: {taskItem.TaskId}, Description: {taskItem.Description}");
        
        // Validate that the task exists
        var taskExists = await _context.Tasks.AnyAsync(t => t.Id == taskItem.TaskId);
        if (!taskExists)
        {
            Console.WriteLine($"❌ Task {taskItem.TaskId} does not exist!");
            return BadRequest(new { message = "Invalid TaskId. Task does not exist." });
        }

        _context.TaskItems.Add(taskItem);
        await _context.SaveChangesAsync();
        
        // DEBUG: Log saved data
        Console.WriteLine($"✅ TaskItem created with ID: {taskItem.Id}, TaskId: {taskItem.TaskId}");

        return CreatedAtAction(nameof(GetTaskItem), new { id = taskItem.Id }, taskItem);
    }

    // PATCH: api/TaskItems/{id}
    [HttpPatch("{id}")]
    public async Task<ActionResult<TaskItem>> UpdateTaskItem(int id, [FromBody] TaskItem taskItem)
    {
        if (id != taskItem.Id)
        {
            return BadRequest(new { message = "ID mismatch" });
        }

        var existingItem = await _context.TaskItems.FindAsync(id);
        if (existingItem == null)
        {
            return NotFound(new { message = "Task item not found" });
        }

        // Update fields
        existingItem.Description = taskItem.Description;
        existingItem.IsCompleted = taskItem.IsCompleted;
        existingItem.TaskId = taskItem.TaskId;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!TaskItemExists(id))
            {
                return NotFound();
            }
            throw;
        }

        return existingItem;
    }

    // PATCH: api/TaskItems/{id}/complete
    [HttpPatch("{id}/complete")]
    public async Task<ActionResult<TaskItem>> CompleteTaskItem(int id)
    {
        var taskItem = await _context.TaskItems.FindAsync(id);
        if (taskItem == null)
        {
            return NotFound(new { message = "Task item not found" });
        }

        taskItem.IsCompleted = true;
        await _context.SaveChangesAsync();

        return taskItem;
    }

    // PATCH: api/TaskItems/{id}/incomplete
    [HttpPatch("{id}/incomplete")]
    public async Task<ActionResult<TaskItem>> IncompleteTaskItem(int id)
    {
        var taskItem = await _context.TaskItems.FindAsync(id);
        if (taskItem == null)
        {
            return NotFound(new { message = "Task item not found" });
        }

        taskItem.IsCompleted = false;
        await _context.SaveChangesAsync();

        return taskItem;
    }

    // DELETE: api/TaskItems/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTaskItem(int id)
    {
        var taskItem = await _context.TaskItems.FindAsync(id);
        if (taskItem == null)
        {
            return NotFound(new { message = "Task item not found" });
        }

        _context.TaskItems.Remove(taskItem);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool TaskItemExists(int id)
    {
        return _context.TaskItems.Any(e => e.Id == id);
    }
}