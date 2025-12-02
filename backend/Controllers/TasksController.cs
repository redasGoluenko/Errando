using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Errando.Data;
using System.Security.Claims;  // ← ADD THIS LINE

namespace backend.Controllers;

[Authorize]
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
    public async Task<ActionResult<IEnumerable<object>>> GetTasks()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

        IQueryable<TodoTask> query = _context.Tasks
            .Include(t => t.Client)
            .Include(t => t.Runner);

        // Filter based on role
        if (userRole == "Client")
        {
            // Clients see only their own tasks
            query = query.Where(t => t.ClientId == userId);
        }
        else if (userRole == "Runner")
        {
            // Runners see unassigned tasks + their assigned tasks
            query = query.Where(t => t.RunnerId == null || t.RunnerId == userId);
        }
        // Admin sees all tasks (no filter)

        var tasks = await query.ToListAsync();

        return Ok(tasks.Select(t => new
        {
            id = t.Id,
            title = t.Title,
            description = t.Description,
            scheduledTime = t.ScheduledTime,
            status = t.Status,
            clientId = t.ClientId,
            clientUsername = t.Client?.Username,
            runnerId = t.RunnerId,
            runnerUsername = t.Runner?.Username,
            createdAt = t.CreatedAt,
            updatedAt = t.UpdatedAt
        }));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetTask(int id)
    {
        var task = await _context.Tasks
            .Include(t => t.Client)
            .Include(t => t.Runner)  // ← Make sure this is here
            .FirstOrDefaultAsync(t => t.Id == id);

        if (task == null)
        {
            return NotFound();
        }

        return Ok(new
        {
            id = task.Id,
            title = task.Title,
            description = task.Description,
            scheduledTime = task.ScheduledTime,
            status = task.Status,
            clientId = task.ClientId,
            clientUsername = task.Client?.Username,
            runnerId = task.RunnerId,
            runnerUsername = task.Runner?.Username,  // ← ADD THIS
            createdAt = task.CreatedAt,
            updatedAt = task.UpdatedAt
        });
    }

    [HttpPost]
    public async Task<ActionResult<TodoTask>> CreateTask([FromBody] TodoTask task) // ← CHANGED
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

        if (userRole == "Client")
        {
            task.ClientId = userId;
        }

        var clientExists = await _context.Users.AnyAsync(u => u.Id == task.ClientId);
        if (!clientExists)
        {
            return BadRequest(new { message = "Invalid ClientId. User does not exist." });
        }

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult<TodoTask>> UpdateTask(int id, [FromBody] TodoTask task) // ← CHANGED
    {
        if (id != task.Id)
        {
            return BadRequest(new { message = "ID mismatch" });
        }

        var existingTask = await _context.Tasks.FindAsync(id);
        if (existingTask == null)
        {
            return NotFound(new { message = "Task not found" });
        }

        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

        if (userRole == "Client" && existingTask.ClientId != userId)
        {
            return Forbid();
        }

        existingTask.Title = task.Title;
        existingTask.Description = task.Description;
        existingTask.ScheduledTime = task.ScheduledTime;

        if (userRole == "Admin")
        {
            existingTask.ClientId = task.ClientId;
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!TaskExists(id))
            {
                return NotFound();
            }
            throw;
        }

        return existingTask;
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task == null)
        {
            return NotFound(new { message = "Task not found" });
        }

        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

        if (userRole == "Client" && task.ClientId != userId)
        {
            return Forbid();
        }

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool TaskExists(int id)
    {
        return _context.Tasks.Any(e => e.Id == id);
    }

    // PATCH: api/Tasks/{id}/assign - Runner assigns task to themselves
    [HttpPatch("{id}/assign")]
    [Authorize(Roles = "Runner")]
    public async Task<ActionResult<TodoTask>> AssignTask(int id)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task == null)
        {
            return NotFound(new { message = "Task not found" });
        }

        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

        // Check if already assigned
        if (task.RunnerId.HasValue)
        {
            return BadRequest(new { message = "Task is already assigned to a runner" });
        }

        task.RunnerId = userId;
        await _context.SaveChangesAsync();

        return task;
    }

    // PATCH: api/Tasks/{id}/unassign - Runner unassigns themselves
    [HttpPatch("{id}/unassign")]
    [Authorize(Roles = "Runner,Admin")]
    public async Task<ActionResult<TodoTask>> UnassignTask(int id)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task == null)
        {
            return NotFound(new { message = "Task not found" });
        }

        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

        // Runner can only unassign their own tasks, Admin can unassign any
        if (userRole == "Runner" && task.RunnerId != userId)
        {
            return Forbid();
        }

        task.RunnerId = null;
        await _context.SaveChangesAsync();

        return task;
    }
}