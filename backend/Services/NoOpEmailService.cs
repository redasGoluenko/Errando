namespace Errando.Services;

public class NoOpEmailService : IEmailService
{
    private readonly ILogger<NoOpEmailService> _logger;

    public NoOpEmailService(ILogger<NoOpEmailService> logger)
    {
        _logger = logger;
    }

    public Task<bool> SendRegistrationConfirmationAsync(string email, string username)
    {
        _logger.LogWarning($"Email service not configured. Would have sent registration confirmation to {email} for user {username}");
        return Task.FromResult(true);
    }

    public Task<bool> SendTaskExpirationAsync(string email, string taskTitle, DateTime expirationDate)
    {
        _logger.LogWarning($"Email service not configured. Would have sent task expiration notification to {email} for task '{taskTitle}' that expired on {expirationDate}");
        return Task.FromResult(true);
    }

    public Task<bool> SendTaskCreatedAsync(string email, string taskTitle, string clientUsername, DateTime scheduledTime)
    {
        _logger.LogWarning($"Email service not configured. Would have sent task created notification to {email} for task '{taskTitle}' created by {clientUsername} scheduled for {scheduledTime}");
        return Task.FromResult(true);
    }

    public Task<bool> SendTaskCompletedAsync(string email, string taskTitle, string clientUsername)
    {
        _logger.LogWarning($"Email service not configured. Would have sent task completed notification to {email} for task '{taskTitle}' completed by {clientUsername}");
        return Task.FromResult(true);
    }
}
