using System.ComponentModel.DataAnnotations;

public class User
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Username must be between 1 and 100 characters")]
    public string Username { get; set; } = null!;
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
    
    [Required]
    public string PasswordHash { get; set; } = null!;
    
    public string Role { get; set; } = "Client";
}
