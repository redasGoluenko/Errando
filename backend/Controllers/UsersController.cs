using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Errando.Data;
using Errando.DTOs;
using Errando.Services;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/[controller]")]
[Authorize] // visiems endpointams reikia autentifikacijos, išskyrus pažymėtus AllowAnonymous
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;
    private readonly IEmailService _emailService;

    public UsersController(AppDbContext context, IConfiguration config, IEmailService emailService)
    {
        _context = context;
        _config = config;
        _emailService = emailService;
    }

    // DTO'ai lokalūs – trumpam sprendimui; galima perkelti į atskirą failą
    public class CreateUserDto
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Role { get; set; }
    }

    public class UpdateUserDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Password { get; set; } // plain-text optional
        public string? Role { get; set; }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        // Admin sees all users
        if (User.IsInRole("Admin"))
        {
            var users = await _context.Users.ToListAsync();
            users.ForEach(u => u.PasswordHash = string.Empty);
            return users;
        }

        // Clients/Runners see only their own user
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(claim, out var currentUserId)) return Forbid();

        var user = await _context.Users.FindAsync(currentUserId);
        if (user == null) return NotFound();

        user.PasswordHash = string.Empty;
        return new List<User> { user };
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<User>> GetUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

        // Admin can view any user
        if (User.IsInRole("Admin"))
        {
            user.PasswordHash = string.Empty;
            return user;
        }

        // Client/Runner can view only their own user
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(claim, out var currentUserId)) return Forbid();
        if (currentUserId != id) return Forbid();

        user.PasswordHash = string.Empty;
        return user;
    }

    // Viešas Create (admin funkcija) – priima DTO su plain password
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<User>> CreateUser(CreateUserDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Username))
            return BadRequest("Username is required and cannot be empty");
        if (string.IsNullOrWhiteSpace(dto.Email))
            return BadRequest("Email is required and cannot be empty");
        if (string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest("Password is required");

        if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
            return BadRequest("Username already taken");

        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = string.IsNullOrEmpty(dto.Role) ? "Client" : dto.Role
        };

        try
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            user.PasswordHash = string.Empty;
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }
        catch (DbUpdateException ex)
        {
            return BadRequest($"Error creating user: {ex.InnerException?.Message ?? ex.Message}");
        }
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<User>> Register(RegisterDto dto)
    {
        if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
            return BadRequest("Username already taken");

        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = "Client"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Send registration confirmation email (fire and forget, but log errors)
        try
        {
            await _emailService.SendRegistrationConfirmationAsync(user.Email, user.Username);
        }
        catch (Exception ex)
        {
            // Log but don't fail the registration
            System.Diagnostics.Debug.WriteLine($"Email sending error: {ex}");
        }

        user.PasswordHash = string.Empty; // nerodyti hash klientui

        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == dto.Username);
        if (user == null) return Unauthorized("Invalid credentials");

        var storedHash = user.PasswordHash ?? string.Empty;

        // paprasta validacija: bcrypt hash paprastai prasideda "$2"
        if (string.IsNullOrWhiteSpace(storedHash) || !storedHash.StartsWith("$2"))
        {
            // jei DB turi nehash'intą/slaptą reikšmę, nepripažinti prisijungimo
            return Unauthorized("Invalid credentials");
        }

        try
        {
            if (!BCrypt.Net.BCrypt.Verify(dto.Password, storedHash))
                return Unauthorized("Invalid credentials");
        }
        catch (BCrypt.Net.SaltParseException)
        {
            // netinkamas saugomas hash — ne 500 klientui
            return Unauthorized("Invalid credentials");
        }

        var jwtSecret = _config["Jwt:Key"] ?? Environment.GetEnvironmentVariable("JWT_SECRET");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret ?? string.Empty));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return new AuthResponse
        {
            Token = tokenString,
            UserId = user.Id,
            Username = user.Username,
            Role = user.Role
        };
    }

    // admin-only: kur kuriami/trinami vartotojai
    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

        try
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (DbUpdateException)
        {
            return Conflict("Cannot delete user because they have associated tasks or other dependencies");
        }
    }

    // Update: leidžiama Admin arba pats vartotojas
    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateUser(int id, UpdateUserDto dto)
    {
        if (id != dto.Id) return BadRequest("ID mismatch");
        if (string.IsNullOrWhiteSpace(dto.Username))
            return BadRequest("Username is required and cannot be empty");

        var existingUser = await _context.Users.FindAsync(id);
        if (existingUser == null) return NotFound();

        // allow only Admin or the user themself
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!User.IsInRole("Admin") && currentUserId != id.ToString())
            return Forbid();

        existingUser.Username = dto.Username;
        existingUser.Email = dto.Email;

        // Only Admin may change roles
        if (User.IsInRole("Admin") && dto.Role != null)
        {
            existingUser.Role = dto.Role;
        }

        // jeigu kliento DTO turi password (plain text) — hash'inti ir saugoti
        if (!string.IsNullOrEmpty(dto.Password))
        {
            existingUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        }

        try
        {
            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Users.AnyAsync(u => u.Id == id))
                return NotFound();
            throw;
        }
    }

    // GET: All runners with their stats (visible to all authenticated users)
    [HttpGet("runners/stats")]
    public async Task<ActionResult<IEnumerable<RunnerStatsDto>>> GetRunnerStats()
    {
        var runners = await _context.Users
            .Where(u => u.Role == "Runner")
            .ToListAsync();

        var tasks = await _context.Tasks
            .Include(t => t.TaskItems)
            .ToListAsync();

        var complaints = await _context.Complaints.ToListAsync();

        // A task is completed if all its task items are completed
        var completedTasks = tasks
            .Where(t => t.TaskItems.Count > 0 && t.TaskItems.All(ti => ti.IsCompleted))
            .ToList();

        var allAssignedTasks = tasks.Where(t => t.RunnerId.HasValue).ToList();

        var runnerStats = runners.Select(runner => new RunnerStatsDto
        {
            Id = runner.Id,
            Username = runner.Username,
            Rating = runner.AverageRating,
            TotalReviews = runner.TotalReviews,
            TasksCompleted = completedTasks.Count(t => t.RunnerId == runner.Id),
            ActiveTasks = tasks.Count(t => t.RunnerId == runner.Id && (t.TaskItems.Count == 0 || !t.TaskItems.All(ti => ti.IsCompleted))),
            MoneyEarned = completedTasks
                .Where(t => t.RunnerId == runner.Id)
                .Sum(t => t.Price ?? 0),
            TotalTasksAssigned = allAssignedTasks.Count(t => t.RunnerId == runner.Id),
            TaskAcceptanceRate = allAssignedTasks.Count(t => t.RunnerId == runner.Id) > 0 
                ? Math.Round((decimal)completedTasks.Count(t => t.RunnerId == runner.Id) / 
                    allAssignedTasks.Count(t => t.RunnerId == runner.Id) * 100, 2)
                : 0
        })
        .OrderByDescending(r => r.Rating)
        .ToList();

        return Ok(runnerStats);
    }

    /// <summary>
    /// Get statistics for all clients (rating and completed tasks)
    /// </summary>
    [HttpGet("clients/stats")]
    public async Task<ActionResult<IEnumerable<ClientStatsDto>>> GetClientStats()
    {
        var clients = await _context.Users
            .Where(u => u.Role == "Client")
            .ToListAsync();

        var tasks = await _context.Tasks
            .Include(t => t.TaskItems)
            .ToListAsync();

        var payments = await _context.Payments.ToListAsync();
        var complaints = await _context.Complaints.ToListAsync();

        // A task is completed if all its task items are completed
        var completedTasks = tasks
            .Where(t => t.TaskItems.Count > 0 && t.TaskItems.All(ti => ti.IsCompleted))
            .ToList();

        var clientStats = clients.Select(client => new ClientStatsDto
        {
            Id = client.Id,
            Username = client.Username,
            Rating = client.AverageRating,
            TotalReviews = client.TotalReviews,
            TasksCreated = tasks.Count(t => t.ClientId == client.Id),
            TasksCompleted = completedTasks.Count(t => t.ClientId == client.Id),
            ActiveTasks = tasks.Count(t => t.ClientId == client.Id && (t.TaskItems.Count == 0 || !t.TaskItems.All(ti => ti.IsCompleted))),
            TotalSpent = payments
                .Where(p => p.ClientId == client.Id && p.Status == "succeeded")
                .Sum(p => p.Amount),
            ComplaintsFiled = complaints.Count(c => c.ClientId == client.Id),
            CompletionRate = tasks.Count(t => t.ClientId == client.Id) > 0
                ? Math.Round((decimal)completedTasks.Count(t => t.ClientId == client.Id) / 
                    tasks.Count(t => t.ClientId == client.Id) * 100, 2)
                : 0
        })
        .OrderByDescending(c => c.Rating)
        .ToList();

        return Ok(clientStats);
    }

    /// <summary>
    /// Get admin dashboard statistics
    /// </summary>
    [HttpGet("admin/stats")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<AdminStatsDto>> GetAdminStats()
    {
        var users = await _context.Users.ToListAsync();
        var tasks = await _context.Tasks
            .Include(t => t.TaskItems)
            .ToListAsync();
        var complaints = await _context.Complaints.ToListAsync();
        var payments = await _context.Payments.ToListAsync();

        // Calculate task statistics
        var completedTasks = tasks
            .Where(t => t.TaskItems.Count > 0 && t.TaskItems.All(ti => ti.IsCompleted))
            .ToList();
        
        var activeTasks = tasks.Where(t => t.TaskItems.Count == 0 || !t.TaskItems.All(ti => ti.IsCompleted)).ToList();
        
        var activeUsers = users.Where(u => !string.IsNullOrEmpty(u.Username)).ToList();
        
        var thisMonth = DateTime.UtcNow.AddMonths(-1);
        var usersCreatedThisMonth = users.Count();
        var tasksCreatedThisMonth = tasks.Count(t => t.CreatedAt >= thisMonth);

        var successfulPayments = payments.Where(p => p.Status == "succeeded").ToList();
        var pendingPayments = payments.Where(p => p.Status == "pending").ToList();
        var failedPayments = payments.Where(p => p.Status == "failed").ToList();

        var totalRevenue = successfulPayments.Sum(p => p.Amount);
        var resolvedComplaints = complaints.Where(c => c.IsResolved).ToList();
        var unresolvedComplaints = complaints.Where(c => !c.IsResolved).ToList();

        var averageRating = users.Where(u => u.AverageRating > 0).Count() > 0
            ? Math.Round(users.Average(u => u.AverageRating), 2)
            : 0m;

        return Ok(new AdminStatsDto
        {
            TotalUsers = users.Count,
            ActiveUsers = activeUsers.Count,
            AdminCount = users.Count(u => u.Role == "Admin"),
            ClientCount = users.Count(u => u.Role == "Client"),
            RunnerCount = users.Count(u => u.Role == "Runner"),
            
            TotalTasks = tasks.Count,
            CompletedTasks = completedTasks.Count,
            ActiveTasks = activeTasks.Count,
            TaskCompletionRate = tasks.Count > 0 
                ? Math.Round((decimal)completedTasks.Count / tasks.Count * 100, 2)
                : 0,
            
            TotalComplaints = complaints.Count,
            ResolvedComplaints = resolvedComplaints.Count,
            UnresolvedComplaints = unresolvedComplaints.Count,
            ComplaintResolutionRate = complaints.Count > 0
                ? Math.Round((decimal)resolvedComplaints.Count / complaints.Count * 100, 2)
                : 0,
            
            TotalRevenue = totalRevenue,
            SuccessfulPayments = successfulPayments.Count,
            PendingPayments = pendingPayments.Count,
            FailedPayments = failedPayments.Count,
            
            AverageSystemRating = averageRating,
            UsersCreatedThisMonth = usersCreatedThisMonth,
            TasksCreatedThisMonth = tasksCreatedThisMonth
        });
    }
}
