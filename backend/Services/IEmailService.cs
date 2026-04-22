namespace Errando.Services;

public interface IEmailService
{
    Task<bool> SendRegistrationConfirmationAsync(string email, string username);
}
