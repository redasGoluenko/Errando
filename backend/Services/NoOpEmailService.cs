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
}
