using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Errando.Data;
using Errando.DTOs;
using Errando.Services;
using System.Security.Claims;
using System.Globalization;

namespace backend.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IImageStorageService _imageStorageService;
    private readonly ILogger<TasksController> _logger;

    public TasksController(AppDbContext context, IEmailService emailService, IImageStorageService imageStorageService, ILogger<TasksController> logger)
    {
        _context = context;
        _emailService = emailService;
        _imageStorageService = imageStorageService;
        _logger = logger;
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
            // Clients see only their own tasks, excluding ones they've deleted
            query = query.Where(t => t.ClientId == userId && !t.IsDeletedByClient);
        }
        else if (userRole == "Runner")
        {
            // Runners see unassigned tasks + their assigned tasks, excluding ones they've deleted
            query = query.Where(t => (t.RunnerId == null || t.RunnerId == userId) && !t.IsDeletedByRunner);
        }
        // Admin sees all tasks (no filter)

        // Filter out expired tasks (soft-delete)
        query = query.Where(t => !t.IsExpired);

        var tasks = await query.Include(t => t.TaskItems).ToListAsync();

        // Mark any tasks that have reached their expiration date
        await MarkExpiredTasks(tasks);

        return Ok(tasks.Where(t => !t.IsExpired).Select(t => new
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
            location = t.Location,
            price = t.Price,
            isRecurring = t.IsRecurring,
            recurringDayOfWeek = t.RecurringDayOfWeek,
            recurringRepetitions = t.RecurringRepetitions,
            expirationDate = t.ExpirationDate,
            isExpired = t.IsExpired,
            photoUrl = t.PhotoUrl,
            createdAt = t.CreatedAt,
            updatedAt = t.UpdatedAt,
            isCompleted = t.TaskItems.Count > 0 && t.TaskItems.All(ti => ti.IsCompleted)
        }));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetTask(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

        var task = await _context.Tasks
            .Include(t => t.Client)
            .Include(t => t.Runner)
            .Include(t => t.TaskItems)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (task == null)
        {
            return NotFound();
        }

        // Check if current user has soft-deleted this task
        if (userRole == "Client" && task.IsDeletedByClient)
        {
            return NotFound();
        }

        if (userRole == "Runner" && task.IsDeletedByRunner)
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
            runnerUsername = task.Runner?.Username,
            location = task.Location,
            price = task.Price,
            isRecurring = task.IsRecurring,
            recurringDayOfWeek = task.RecurringDayOfWeek,
            recurringRepetitions = task.RecurringRepetitions,
            expirationDate = task.ExpirationDate,
            isExpired = task.IsExpired,
            photoUrl = task.PhotoUrl,
            createdAt = task.CreatedAt,
            updatedAt = task.UpdatedAt,
            isCompleted = task.TaskItems.Count > 0 && task.TaskItems.All(ti => ti.IsCompleted)
        });
    }

    [HttpPost]
    public async Task<ActionResult<object>> CreateTask([FromBody] CreateTaskDto createTaskDto)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

        // Check if user is Client/Admin
        if (userRole == "Client")
        {
            createTaskDto.ClientId = userId;
        }

        var clientExists = await _context.Users.AnyAsync(u => u.Id == createTaskDto.ClientId);
        if (!clientExists)
        {
            return BadRequest(new { message = "Invalid ClientId. User does not exist." });
        }

        // Validate periodicity fields
        if (createTaskDto.IsRecurring)
        {
            if (!createTaskDto.RecurringDayOfWeek.HasValue || createTaskDto.RecurringDayOfWeek < 0 || createTaskDto.RecurringDayOfWeek > 6)
            {
                return BadRequest(new { message = "Valid RecurringDayOfWeek (0-6) is required when IsRecurring is true." });
            }

            if (!createTaskDto.RecurringRepetitions.HasValue || createTaskDto.RecurringRepetitions <= 0)
            {
                return BadRequest(new { message = "RecurringRepetitions must be greater than 0 when IsRecurring is true." });
            }
        }

        var createdTasks = new List<TodoTask>();

        if (createTaskDto.IsRecurring && createTaskDto.RecurringRepetitions.HasValue && createTaskDto.RecurringDayOfWeek.HasValue)
        {
            // Create recurring tasks
            var targetDayOfWeek = (DayOfWeek)createTaskDto.RecurringDayOfWeek.Value;
            var currentDate = DateTime.SpecifyKind(createTaskDto.ScheduledTime.Date, DateTimeKind.Utc);

            // Find the first occurrence of the target day of week
            while (currentDate.DayOfWeek != targetDayOfWeek)
            {
                currentDate = currentDate.AddDays(1);
            }

            // Create tasks for each repetition
            for (int i = 0; i < createTaskDto.RecurringRepetitions.Value; i++)
            {
                var newTask = new TodoTask
                {
                    Title = createTaskDto.Title,
                    Description = createTaskDto.Description,
                    ScheduledTime = currentDate.Add(createTaskDto.ScheduledTime.TimeOfDay),
                    ClientId = createTaskDto.ClientId,
                    Location = createTaskDto.Location,
                    Price = createTaskDto.Price,
                    PhotoUrl = createTaskDto.PhotoUrl,
                    IsRecurring = false,  // Individual tasks are not marked as recurring
                    ExpirationDate = createTaskDto.ExpirationDate,
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Tasks.Add(newTask);
                createdTasks.Add(newTask);
                currentDate = currentDate.AddDays(7);  // Move to next week
            }
        }
        else
        {
            // Create single task
            var newTask = new TodoTask
            {
                Title = createTaskDto.Title,
                Description = createTaskDto.Description,
                ScheduledTime = createTaskDto.ScheduledTime,
                ClientId = createTaskDto.ClientId,
                Location = createTaskDto.Location,
                Price = createTaskDto.Price,
                PhotoUrl = createTaskDto.PhotoUrl,
                IsRecurring = createTaskDto.IsRecurring,
                RecurringDayOfWeek = createTaskDto.RecurringDayOfWeek,
                RecurringRepetitions = createTaskDto.RecurringRepetitions,
                ExpirationDate = createTaskDto.ExpirationDate,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Tasks.Add(newTask);
            createdTasks.Add(newTask);
        }

        await _context.SaveChangesAsync();

        // Send email notification to the client(s) who created the task
        var client = await _context.Users.FirstOrDefaultAsync(u => u.Id == createTaskDto.ClientId);
        if (client != null)
        {
            foreach (var task in createdTasks)
            {
                try
                {
                    _logger.LogInformation($"[DEBUG] Sending task created email to {client.Email} for task '{task.Title}'");
                    var result = await _emailService.SendTaskCreatedAsync(client.Email, task.Title, client.Username, task.ScheduledTime);
                    _logger.LogInformation($"[DEBUG] Email send result: {result}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"[DEBUG] Exception sending task created email to {client.Email}: {ex.Message}");
                }
            }
        }
        else
        {
            _logger.LogWarning($"[DEBUG] Client with ID {createTaskDto.ClientId} not found!");
        }

        // Return the created tasks
        if (createdTasks.Count == 1)
        {
            return CreatedAtAction(nameof(GetTask), new { id = createdTasks[0].Id }, new
            {
                id = createdTasks[0].Id,
                title = createdTasks[0].Title,
                description = createdTasks[0].Description,
                scheduledTime = createdTasks[0].ScheduledTime,
                status = createdTasks[0].Status,
                clientId = createdTasks[0].ClientId,
                location = createdTasks[0].Location,
                price = createdTasks[0].Price,
                isRecurring = createdTasks[0].IsRecurring,
                expirationDate = createdTasks[0].ExpirationDate,
                createdAt = createdTasks[0].CreatedAt
            });
        }
        else
        {
            return Ok(new
            {
                message = $"Created {createdTasks.Count} recurring tasks",
                tasks = createdTasks.Select(t => new
                {
                    id = t.Id,
                    title = t.Title,
                    description = t.Description,
                    scheduledTime = t.ScheduledTime,
                    status = t.Status,
                    clientId = t.ClientId,
                    location = t.Location,
                    price = t.Price,
                    createdAt = t.CreatedAt
                })
            });
        }
    }

    [HttpPatch("{id}")]
    public async Task<ActionResult<TodoTask>> UpdateTask(int id, [FromBody] UpdateTaskDto updateTaskDto)
    {
        if (id != updateTaskDto.Id)
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

        existingTask.Title = updateTaskDto.Title;
        existingTask.Description = updateTaskDto.Description;
        existingTask.ScheduledTime = updateTaskDto.ScheduledTime;
        existingTask.Location = updateTaskDto.Location;
        existingTask.Price = updateTaskDto.Price;
            existingTask.ExpirationDate = updateTaskDto.ExpirationDate;

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
        var task = await _context.Tasks
            .Include(t => t.TaskItems)
            .FirstOrDefaultAsync(t => t.Id == id);
            
        if (task == null)
        {
            return NotFound(new { message = "Task not found" });
        }

        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

        // Admin can delete any task permanently (hard delete)
        if (userRole == "Admin")
        {
            try
            {
                // Delete related payments (cascade should handle this, but explicit for clarity)
                var payments = await _context.Payments
                    .Where(p => p.TaskId == id)
                    .ToListAsync();
                _context.Payments.RemoveRange(payments);

                // Delete related complaints (cascade should handle this)
                var complaints = await _context.Complaints
                    .Where(c => c.TaskId == id)
                    .ToListAsync();
                _context.Complaints.RemoveRange(complaints);

                // Delete related reviews (cascade should handle this)
                var reviews = await _context.Reviews
                    .Where(r => r.TaskId == id)
                    .ToListAsync();
                _context.Reviews.RemoveRange(reviews);

                // Delete the task itself (TaskItems will be cascade deleted by EF)
                _context.Tasks.Remove(task);
                
                await _context.SaveChangesAsync();
                
                return Ok(new { message = "Task permanently deleted" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting task {id}: {ex.Message}");
                return StatusCode(500, new { message = "Error deleting task", error = ex.Message });
            }
        }

        if (userRole == "Client" && task.ClientId != userId)
        {
            return Forbid();
        }

        if (userRole == "Runner" && task.RunnerId != userId)
        {
            return Forbid();
        }

        // Clients can only delete tasks they have paid for
        if (userRole == "Client")
        {
            var payment = await _context.Payments.FirstOrDefaultAsync(p => p.TaskId == id && p.Status == "succeeded");
            if (payment == null)
            {
                return BadRequest(new { message = "You can only delete tasks after payment has been completed." });
            }
            // Only proceed with soft delete after payment is verified
            task.IsDeletedByClient = true;
        }
        else if (userRole == "Runner")
        {
            // Runners can delete tasks freely
            task.IsDeletedByRunner = true;
        }

        _context.Tasks.Update(task);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Task deleted" });
    }

    [HttpPost("upload-photo")]
    [Authorize]
    public async Task<ActionResult<object>> UploadTaskPhoto(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "No file provided" });
        }

        try
        {
            using (var stream = file.OpenReadStream())
            {
                var photoUrl = await _imageStorageService.UploadImageAsync(stream, file.FileName, file.ContentType);
                
                if (photoUrl == null)
                {
                    return BadRequest(new { message = "Failed to upload image. Check file size (max 1MB) and format (JPG, PNG, WebP, GIF)" });
                }

                return Ok(new { photoUrl });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading task photo");
            return StatusCode(500, new { message = "Error uploading photo" });
        }
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

    // Helper method to mark tasks as expired if their expiration date has passed
    private async Task MarkExpiredTasks(List<TodoTask> tasks)
    {
        var now = DateTime.UtcNow;
        var tasksToMarkExpired = tasks.Where(t => 
            t.ExpirationDate.HasValue && 
            t.ExpirationDate <= now && 
            !t.IsExpired
        ).ToList();

        if (tasksToMarkExpired.Any())
        {
            // Fetch client information for email sending
            var clientIds = tasksToMarkExpired.Select(t => t.ClientId).Distinct().ToList();
            var clients = await _context.Users
                .Where(u => clientIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u);

            foreach (var task in tasksToMarkExpired)
            {
                task.IsExpired = true;

                // Send expiration notification email to client
                if (clients.TryGetValue(task.ClientId, out var client) && !string.IsNullOrEmpty(client.Email) && task.ExpirationDate.HasValue)
                {
                    await _emailService.SendTaskExpirationAsync(client.Email, task.Title, task.ExpirationDate.Value);
                }
            }
            await _context.SaveChangesAsync();
        }
    }
}