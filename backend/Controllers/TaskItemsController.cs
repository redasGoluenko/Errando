using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Errando.Data;
using System.Security.Claims;

namespace Errando.Controllers
{
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
        public async Task<ActionResult<IEnumerable<object>>> GetTaskItems([FromQuery] int taskId)
        {
            var taskItems = await _context.TaskItems
                .Where(ti => ti.TaskId == taskId)
                .OrderBy(ti => ti.Id)
                .ToListAsync();

            return Ok(taskItems.Select(ti => new
            {
                id = ti.Id,
                taskId = ti.TaskId,
                description = ti.Description,
                isCompleted = ti.IsCompleted
            }));
        }

        [HttpPost]
        [Authorize(Roles = "Client,Admin")]
        public async Task<ActionResult<object>> CreateTaskItem(CreateTaskItemDto dto)
        {
            var task = await _context.Tasks.FindAsync(dto.TaskId);
            if (task == null)
            {
                return NotFound(new { message = "Task not found" });
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (userRole != "Admin" && task.ClientId != userId)
            {
                return Forbid();
            }

            var taskItem = new TaskItem
            {
                TaskId = dto.TaskId,
                Description = dto.Description,
                IsCompleted = dto.IsCompleted
            };

            _context.TaskItems.Add(taskItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTaskItems), new { taskId = taskItem.TaskId }, new
            {
                id = taskItem.Id,
                taskId = taskItem.TaskId,
                description = taskItem.Description,
                isCompleted = taskItem.IsCompleted
            });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Runner,Client,Admin")]
        public async Task<ActionResult<object>> UpdateTaskItem(int id, UpdateTaskItemDto dto)
        {
            var taskItem = await _context.TaskItems
                .Include(ti => ti.Task)
                .FirstOrDefaultAsync(ti => ti.Id == id);

            if (taskItem == null)
            {
                return NotFound(new { message = "Task item not found" });
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            // Check permissions: Client who owns the task, Runner assigned to task, or Admin
            if (userRole != "Admin" && 
                taskItem.Task.ClientId != userId && 
                taskItem.Task.RunnerId != userId)
            {
                return Forbid();
            }

            taskItem.Description = dto.Description;
            taskItem.IsCompleted = dto.IsCompleted;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                id = taskItem.Id,
                taskId = taskItem.TaskId,
                description = taskItem.Description,
                isCompleted = taskItem.IsCompleted
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Client,Admin")]
        public async Task<IActionResult> DeleteTaskItem(int id)
        {
            var taskItem = await _context.TaskItems
                .Include(ti => ti.Task)
                .FirstOrDefaultAsync(ti => ti.Id == id);

            if (taskItem == null)
            {
                return NotFound(new { message = "Task item not found" });
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (userRole != "Admin" && taskItem.Task.ClientId != userId)
            {
                return Forbid();
            }

            _context.TaskItems.Remove(taskItem);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Task item deleted successfully" });
        }
    }

    public class CreateTaskItemDto
    {
        public int TaskId { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
    }

    public class UpdateTaskItemDto
    {
        public string Description { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
    }
}