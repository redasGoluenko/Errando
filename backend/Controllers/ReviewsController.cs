using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Errando.Data;
using Errando.DTOs;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace backend.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ReviewsController(AppDbContext context)
    {
        _context = context;
    }

    // Lithuanian keywords for sentiment analysis
    private readonly Dictionary<string, decimal> PositiveKeywords = new()
    {
        { "puikiai", 0.5m },           // great
        { "gražiai", 0.4m },           // beautifully
        { "greitai", 0.3m },           // quickly
        { "šauniai", 0.5m },           // awesomely
        { "tobulai", 0.5m },           // perfectly
        { "nuostabu", 0.4m },          // wonderful
        { "nepaperastai", 0.4m },      // extraordinary
        { "išskirtinai", 0.4m },       // exceptionally
        { "liaupsėtine", 0.3m },       // praiseworthy
        { "žavinga", 0.3m }            // charming
    };

    private readonly Dictionary<string, decimal> NegativeKeywords = new()
    {
        { "vėlavo", -0.5m },           // was late
        { "blogai", -0.4m },           // badly
        { "atidėliojo", -0.3m },       // delayed
        { "nekompetentingai", -0.5m }, // incompetently
        { "neprofesionaliai", -0.4m }, // unprofessionally
        { "baisiai", -0.4m },          // terribly
        { "prastai", -0.3m },          // poorly
        { "nepakankamai", -0.3m }      // insufficiently
    };

    [HttpPost]
    public async Task<ActionResult<ReviewDto>> CreateReview([FromBody] CreateReviewDto createReviewDto)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        // Validate task exists
        var task = await _context.Tasks
            .Include(t => t.TaskItems)
            .FirstOrDefaultAsync(t => t.Id == createReviewDto.TaskId);

        if (task == null)
        {
            return BadRequest(new { message = "Task not found" });
        }

        // Validate reviewee exists
        var reviewee = await _context.Users.FindAsync(createReviewDto.RevieweeId);
        if (reviewee == null)
        {
            return BadRequest(new { message = "Reviewee not found" });
        }

        // Verify the reviewer is involved in the task
        bool isClient = task.ClientId == userId;
        bool isRunner = task.RunnerId == userId;

        if (!isClient && !isRunner)
        {
            return Forbid();
        }

        // Verify reviewee is the other party in the task
        bool validReviewee = (isClient && task.RunnerId == createReviewDto.RevieweeId) ||
                            (isRunner && task.ClientId == createReviewDto.RevieweeId);

        if (!validReviewee)
        {
            return BadRequest(new { message = "Invalid reviewee for this task" });
        }

        // Calculate final rating
        decimal finalRating = CalculateFinalRating(
            createReviewDto.StarRating,
            createReviewDto.ReviewText,
            task
        );

        // Create the review
        var review = new Review
        {
            TaskId = createReviewDto.TaskId,
            ReviewerId = userId,
            RevieweeId = createReviewDto.RevieweeId,
            StarRating = createReviewDto.StarRating,
            ReviewText = createReviewDto.ReviewText,
            FinalRating = finalRating,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        // Update reviewee's rating
        await UpdateUserRating(createReviewDto.RevieweeId);

        return CreatedAtAction(nameof(GetReview), new { id = review.Id }, new ReviewDto
        {
            Id = review.Id,
            TaskId = review.TaskId,
            ReviewerId = review.ReviewerId,
            RevieweeId = review.RevieweeId,
            RevieweeUsername = reviewee.Username,
            StarRating = review.StarRating,
            ReviewText = review.ReviewText,
            FinalRating = review.FinalRating,
            CreatedAt = review.CreatedAt,
            UpdatedAt = review.UpdatedAt
        });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ReviewDto>> GetReview(int id)
    {
        var review = await _context.Reviews
            .Include(r => r.Reviewer)
            .Include(r => r.Reviewee)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (review == null)
        {
            return NotFound();
        }

        return Ok(new ReviewDto
        {
            Id = review.Id,
            TaskId = review.TaskId,
            ReviewerId = review.ReviewerId,
            ReviewerUsername = review.Reviewer?.Username,
            RevieweeId = review.RevieweeId,
            RevieweeUsername = review.Reviewee?.Username,
            StarRating = review.StarRating,
            ReviewText = review.ReviewText,
            FinalRating = review.FinalRating,
            CreatedAt = review.CreatedAt,
            UpdatedAt = review.UpdatedAt
        });
    }

    [HttpGet("task/{taskId}")]
    public async Task<ActionResult<List<ReviewDto>>> GetReviewsByTask(int taskId)
    {
        var reviews = await _context.Reviews
            .Where(r => r.TaskId == taskId)
            .Include(r => r.Reviewer)
            .Include(r => r.Reviewee)
            .ToListAsync();

        return Ok(reviews.Select(r => new ReviewDto
        {
            Id = r.Id,
            TaskId = r.TaskId,
            ReviewerId = r.ReviewerId,
            ReviewerUsername = r.Reviewer?.Username,
            RevieweeId = r.RevieweeId,
            RevieweeUsername = r.Reviewee?.Username,
            StarRating = r.StarRating,
            ReviewText = r.ReviewText,
            FinalRating = r.FinalRating,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt
        }).ToList());
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<GetUserReviewsDto>> GetUserReviews(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        var reviews = await _context.Reviews
            .Where(r => r.RevieweeId == userId)
            .Include(r => r.Reviewer)
            .ToListAsync();

        return Ok(new GetUserReviewsDto
        {
            UserId = user.Id,
            Username = user.Username,
            AverageRating = user.AverageRating,
            TotalReviews = user.TotalReviews,
            Reviews = reviews.Select(r => new ReviewDto
            {
                Id = r.Id,
                TaskId = r.TaskId,
                ReviewerId = r.ReviewerId,
                ReviewerUsername = r.Reviewer?.Username,
                RevieweeId = r.RevieweeId,
                StarRating = r.StarRating,
                ReviewText = r.ReviewText,
                FinalRating = r.FinalRating,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt
            }).ToList()
        });
    }

    private decimal CalculateFinalRating(int starRating, string? reviewText, TodoTask task)
    {
        // Start with the user's star rating (1-5)
        decimal finalRating = starRating;

        // Analyze review text for sentiment keywords
        if (!string.IsNullOrEmpty(reviewText))
        {
            string lowerText = reviewText.ToLowerInvariant();

            // Check positive keywords
            foreach (var keyword in PositiveKeywords)
            {
                if (lowerText.Contains(keyword.Key))
                {
                    finalRating += keyword.Value;
                }
            }

            // Check negative keywords
            foreach (var keyword in NegativeKeywords)
            {
                if (lowerText.Contains(keyword.Key))
                {
                    finalRating += keyword.Value;
                }
            }
        }

        // Check if task was completed quickly (bonus)
        if (task.UpdatedAt < task.ScheduledTime)
        {
            TimeSpan timeMissed = task.ScheduledTime - task.UpdatedAt;
            // Give bonus based on how much earlier it was completed
            if (timeMissed.TotalDays >= 7)
                finalRating += 0.5m; // Completed a week early
            else if (timeMissed.TotalDays >= 3)
                finalRating += 0.3m; // Completed 3+ days early
            else if (timeMissed.TotalDays >= 1)
                finalRating += 0.2m; // Completed 1+ day early
        }
        else if (task.UpdatedAt > task.ScheduledTime)
        {
            // Penalty if completed late
            TimeSpan timeLate = task.UpdatedAt - task.ScheduledTime;
            if (timeLate.TotalDays >= 7)
                finalRating -= 1m; // More than a week late
            else if (timeLate.TotalDays >= 3)
                finalRating -= 0.5m; // 3+ days late
            else if (timeLate.TotalDays >= 1)
                finalRating -= 0.3m; // 1+ day late
        }

        // Clamp the final rating between 1 and 5
        finalRating = Math.Max(1m, Math.Min(5m, finalRating));

        return Math.Round(finalRating, 2);
    }

    private async Task UpdateUserRating(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return;

        var userReviews = await _context.Reviews
            .Where(r => r.RevieweeId == userId)
            .ToListAsync();

        if (userReviews.Count == 0)
        {
            user.AverageRating = 0m;
            user.TotalReviews = 0;
        }
        else
        {
            // Calculate average of final ratings
            user.AverageRating = (decimal)userReviews.Average(r => r.FinalRating);
            user.TotalReviews = userReviews.Count;
        }

        user.AverageRating = Math.Round(user.AverageRating, 2);

        await _context.SaveChangesAsync();
    }
}
