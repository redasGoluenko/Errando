namespace Errando.DTOs;

public class CreateReviewDto
{
    public int TaskId { get; set; }
    public int RevieweeId { get; set; }
    public int StarRating { get; set; }
    public string? ReviewText { get; set; }
}

public class ReviewDto
{
    public int Id { get; set; }
    public int TaskId { get; set; }
    public int ReviewerId { get; set; }
    public string? ReviewerUsername { get; set; }
    public int RevieweeId { get; set; }
    public string? RevieweeUsername { get; set; }
    public int StarRating { get; set; }
    public string? ReviewText { get; set; }
    public decimal FinalRating { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class GetUserReviewsDto
{
    public int UserId { get; set; }
    public string? Username { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public List<ReviewDto> Reviews { get; set; } = new();
}
