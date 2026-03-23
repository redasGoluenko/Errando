using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Errando.Data;
using Errando.DTOs;
using System.Security.Claims;

namespace backend.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ComplaintsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ComplaintsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<ComplaintDto>>> GetComplaints()
    {
        try
        {
            var complaints = await _context.Complaints
                .Include(c => c.Task)
                .Include(c => c.Client)
                .Include(c => c.Runner)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return Ok(complaints.Select(c => new ComplaintDto
            {
                Id = c.Id,
                Description = c.Description,
                TaskId = c.TaskId,
                TaskTitle = c.Task?.Title ?? string.Empty,
                ClientId = c.ClientId,
                ClientUsername = c.Client?.Username ?? string.Empty,
                RunnerId = c.RunnerId,
                RunnerUsername = c.Runner?.Username ?? string.Empty,
                CreatedAt = c.CreatedAt
            }));
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error fetching complaints: {ex.Message}");
            Console.Error.WriteLine($"Stack trace: {ex.StackTrace}");
            return StatusCode(500, new { message = "Error fetching complaints", details = ex.Message });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Client")]
    public async Task<ActionResult<ComplaintDto>> CreateComplaint([FromBody] CreateComplaintDto dto)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        // Verify task exists and belongs to the client
        var task = await _context.Tasks
            .Include(t => t.TaskItems)
            .FirstOrDefaultAsync(t => t.Id == dto.TaskId);
        if (task == null)
        {
            return NotFound(new { message = "Task not found" });
        }

        if (task.ClientId != userId)
        {
            return Forbid();
        }

        // Verify task is completed
        if (task.TaskItems.Count == 0 || !task.TaskItems.All(ti => ti.IsCompleted))
        {
            return BadRequest(new { message = "Can only complain about completed tasks" });
        }

        // Check if runner exists and has an ID
        if (!task.RunnerId.HasValue)
        {
            return BadRequest(new { message = "Task has no assigned runner" });
        }

        // Check if complaint already exists
        var existingComplaint = await _context.Complaints
            .FirstOrDefaultAsync(c => c.TaskId == dto.TaskId && c.ClientId == userId);
        
        if (existingComplaint != null)
        {
            return BadRequest(new { message = "You have already submitted a complaint for this task" });
        }

        var complaint = new Complaint
        {
            Description = dto.Description,
            TaskId = dto.TaskId,
            ClientId = userId,
            RunnerId = task.RunnerId.Value,
            CreatedAt = DateTime.UtcNow
        };

        _context.Complaints.Add(complaint);
        await _context.SaveChangesAsync();

        // Fetch the created complaint with related data
        var createdComplaint = await _context.Complaints
            .Include(c => c.Task)
            .Include(c => c.Client)
            .Include(c => c.Runner)
            .FirstAsync(c => c.Id == complaint.Id);

        return CreatedAtAction(nameof(GetComplaints), new { id = complaint.Id }, new ComplaintDto
        {
            Id = createdComplaint.Id,
            Description = createdComplaint.Description,
            TaskId = createdComplaint.TaskId,
            TaskTitle = createdComplaint.Task?.Title ?? string.Empty,
            ClientId = createdComplaint.ClientId,
            ClientUsername = createdComplaint.Client?.Username ?? string.Empty,
            RunnerId = createdComplaint.RunnerId,
            RunnerUsername = createdComplaint.Runner?.Username ?? string.Empty,
            CreatedAt = createdComplaint.CreatedAt
        });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteComplaint(int id)
    {
        var complaint = await _context.Complaints.FindAsync(id);
        if (complaint == null)
        {
            return NotFound();
        }

        _context.Complaints.Remove(complaint);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
