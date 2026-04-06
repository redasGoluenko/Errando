using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Errando.Data;

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
    [JsonIgnore]
    public string PasswordHash { get; set; } = null!;
    
    public string Role { get; set; } = "Client";

    // Rating fields
    [Column(TypeName = "decimal(3, 2)")]
    public decimal AverageRating { get; set; } = 0m;

    public int TotalReviews { get; set; } = 0;

    // Navigation property
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
