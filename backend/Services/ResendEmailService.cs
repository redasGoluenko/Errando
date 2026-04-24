using System.Text;
using System.Text.Json;

namespace Errando.Services;

public class ResendEmailService : IEmailService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<ResendEmailService> _logger;
    private const string ResendApiUrl = "https://api.resend.com/emails";

    public ResendEmailService(IConfiguration config, ILogger<ResendEmailService> logger)
    {
        var apiKey = config["Resend:ApiKey"] ?? Environment.GetEnvironmentVariable("RESEND_API_KEY");
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException("Resend API key is not configured");
        }

        _apiKey = apiKey;
        _logger = logger;
        _httpClient = new HttpClient();
    }

    public async Task<bool> SendRegistrationConfirmationAsync(string email, string username)
    {
        try
        {
            var htmlContent = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #333;'>Welcome to Errando, {username}!</h2>
                    <p>Thank you for registering with us. Your account has been successfully created.</p>
                    
                    <div style='background-color: #f5f5f5; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                        <p><strong>Account Details:</strong></p>
                        <p>Username: <strong>{username}</strong></p>
                        <p>Role: <strong>Client</strong></p>
                    </div>
                    
                    <p>If you would like to change your role (e.g., to Runner), please contact our administrator.</p>
                    
                    <p style='color: #666; font-size: 12px; margin-top: 30px;'>
                        This is an automated email. Please do not reply to this email address.
                    </p>
                </div>";

            var payload = new
            {
                from = "onboarding@resend.dev",
                to = email,
                subject = "Welcome to Errando - Registration Confirmation",
                html = htmlContent
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var request = new HttpRequestMessage(HttpMethod.Post, ResendApiUrl);
            request.Headers.Add("Authorization", $"Bearer {_apiKey}");
            request.Content = content;

            var response = await _httpClient.SendAsync(request);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Registration confirmation email sent successfully to {email}. Response: {responseContent}");
                return true;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError($"Failed to send registration confirmation email to {email}. Status: {response.StatusCode}, Error: {errorContent}");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Exception occurred while sending registration confirmation email to {email}");
            return false;
        }
    }

    public async Task<bool> SendTaskExpirationAsync(string email, string taskTitle, DateTime expirationDate)
    {
        try
        {
            var formattedDate = expirationDate.ToString("MMMM dd, yyyy 'at' HH:mm");
            
            var htmlContent = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #d32f2f;'>Task Expiration Notification</h2>
                    <p>Your task has reached its expiration date and has been automatically archived.</p>
                    
                    <div style='background-color: #fff3e0; padding: 15px; border-radius: 5px; margin: 20px 0; border-left: 4px solid #ff6f00;'>
                        <p><strong>Task Details:</strong></p>
                        <p>Title: <strong>{taskTitle}</strong></p>
                        <p>Expiration Date: <strong>{formattedDate}</strong></p>
                    </div>
                    
                    <p>This task will no longer appear in your active task list, but you can view it in your task history if needed.</p>
                    <p>If you believe this is an error or need assistance, please contact our support team.</p>
                    
                    <p style='color: #666; font-size: 12px; margin-top: 30px;'>
                        This is an automated email. Please do not reply to this email address.
                    </p>
                </div>";

            var payload = new
            {
                from = "onboarding@resend.dev",
                to = email,
                subject = $"Task Expired: {taskTitle}",
                html = htmlContent
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var request = new HttpRequestMessage(HttpMethod.Post, ResendApiUrl);
            request.Headers.Add("Authorization", $"Bearer {_apiKey}");
            request.Content = content;

            var response = await _httpClient.SendAsync(request);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Task expiration email sent successfully to {email} for task '{taskTitle}'. Response: {responseContent}");
                return true;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError($"Failed to send task expiration email to {email} for task '{taskTitle}'. Status: {response.StatusCode}, Error: {errorContent}");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Exception occurred while sending task expiration email to {email} for task '{taskTitle}'");
            return false;
        }
    }

    public async Task<bool> SendTaskCreatedAsync(string email, string taskTitle, string clientUsername, DateTime scheduledTime)
    {
        try
        {
            var formattedDate = scheduledTime.ToString("MMMM dd, yyyy 'at' HH:mm");
            
            var htmlContent = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #1976d2;'>New Task Created!</h2>
                    <p>A new task has been created and is waiting to be picked up by a runner.</p>
                    
                    <div style='background-color: #e3f2fd; padding: 15px; border-radius: 5px; margin: 20px 0; border-left: 4px solid #1976d2;'>
                        <p><strong>Task Details:</strong></p>
                        <p>Title: <strong>{taskTitle}</strong></p>
                        <p>Scheduled Time: <strong>{formattedDate}</strong></p>
                        <p>Created By: <strong>{clientUsername}</strong></p>
                    </div>
                    
                    <p>You can view this task in your Errando dashboard and manage it as needed. Once a runner accepts the task, they will be assigned to it.</p>
                    
                    <p style='color: #666; font-size: 12px; margin-top: 30px;'>
                        This is an automated email. Please do not reply to this email address.
                    </p>
                </div>";

            var payload = new
            {
                from = "onboarding@resend.dev",
                to = email,
                subject = $"New Task Created: {taskTitle}",
                html = htmlContent
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var request = new HttpRequestMessage(HttpMethod.Post, ResendApiUrl);
            request.Headers.Add("Authorization", $"Bearer {_apiKey}");
            request.Content = content;

            var response = await _httpClient.SendAsync(request);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Task created email sent successfully to {email} for task '{taskTitle}'. Response: {responseContent}");
                return true;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError($"Failed to send task created email to {email} for task '{taskTitle}'. Status: {response.StatusCode}, Error: {errorContent}");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Exception occurred while sending task created email to {email} for task '{taskTitle}'");
            return false;
        }
    }

    public async Task<bool> SendTaskCompletedAsync(string email, string taskTitle, string clientUsername)
    {
        try
        {
            var htmlContent = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #388e3c;'>Task Completed Successfully!</h2>
                    <p>Congratulations! Your task has been completed.</p>
                    
                    <div style='background-color: #e8f5e9; padding: 15px; border-radius: 5px; margin: 20px 0; border-left: 4px solid #388e3c;'>
                        <p><strong>Task Details:</strong></p>
                        <p>Title: <strong>{taskTitle}</strong></p>
                        <p>Completed By: <strong>{clientUsername}</strong></p>
                        <p>Completion Date: <strong>{DateTime.UtcNow:MMMM dd, yyyy 'at' HH:mm}</strong></p>
                    </div>
                    
                    <p>You can view the completed task details in your Errando dashboard. Thank you for using our service!</p>
                    
                    <p style='color: #666; font-size: 12px; margin-top: 30px;'>
                        This is an automated email. Please do not reply to this email address.
                    </p>
                </div>";

            var payload = new
            {
                from = "onboarding@resend.dev",
                to = email,
                subject = $"Task Completed: {taskTitle}",
                html = htmlContent
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var request = new HttpRequestMessage(HttpMethod.Post, ResendApiUrl);
            request.Headers.Add("Authorization", $"Bearer {_apiKey}");
            request.Content = content;

            var response = await _httpClient.SendAsync(request);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Task completed email sent successfully to {email} for task '{taskTitle}'. Response: {responseContent}");
                return true;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError($"Failed to send task completed email to {email} for task '{taskTitle}'. Status: {response.StatusCode}, Error: {errorContent}");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Exception occurred while sending task completed email to {email} for task '{taskTitle}'");
            return false;
        }
    }
}
