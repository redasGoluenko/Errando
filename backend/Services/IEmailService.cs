namespace Errando.Services;

public interface IEmailService
{
    Task<bool> SendRegistrationConfirmationAsync(string email, string username);
    Task<bool> SendTaskExpirationAsync(string email, string taskTitle, DateTime expirationDate);
    Task<bool> SendTaskCreatedAsync(string email, string taskTitle, string clientUsername, DateTime scheduledTime);
    Task<bool> SendTaskCompletedAsync(string email, string taskTitle, string clientUsername);
}
