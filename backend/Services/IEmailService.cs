namespace Errando.Services;

public interface IEmailService
{
    Task<bool> SendRegistrationConfirmationAsync(string email, string username);
    Task<bool> SendTaskExpirationAsync(string email, string taskTitle, DateTime expirationDate);
}
