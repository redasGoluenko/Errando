using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Errando.Data;
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

    public UsersController(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
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
}
