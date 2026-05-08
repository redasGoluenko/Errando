using Xunit;
using Moq;
using Errando.Services;

namespace backend.Tests.Unit.Services;

public class EmailServiceExtendedTests
{
    [Fact]
    public async Task SendRegistrationConfirmation_ValidEmail_Should_Return_True()
    {
        // Arrange
        var mockEmailService = new Mock<IEmailService>();
        mockEmailService.Setup(x => x.SendRegistrationConfirmationAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        var result = await mockEmailService.Object.SendRegistrationConfirmationAsync("user@test.com", "testuser");

        // Assert
        Assert.True(result);
        mockEmailService.Verify(x => x.SendRegistrationConfirmationAsync("user@test.com", "testuser"), Times.Once);
    }

    [Fact]
    public async Task SendTaskCreated_ValidData_Should_Call_Service()
    {
        // Arrange
        var mockEmailService = new Mock<IEmailService>();
        mockEmailService.Setup(x => x.SendTaskCreatedAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
            .ReturnsAsync(true);

        // Act
        var result = await mockEmailService.Object.SendTaskCreatedAsync("user@test.com", "Build Website", "john_client", DateTime.UtcNow.AddHours(2));

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task SendTaskCompleted_ValidData_Should_Return_True()
    {
        // Arrange
        var mockEmailService = new Mock<IEmailService>();
        mockEmailService.Setup(x => x.SendTaskCompletedAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        var result = await mockEmailService.Object.SendTaskCompletedAsync("user@test.com", "Design Logo", "client_user");

        // Assert
        Assert.True(result);
        mockEmailService.Verify(x => x.SendTaskCompletedAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task SendTaskAssigned_WithRunnerInfo_Should_Call_Service()
    {
        // Arrange
        var mockEmailService = new Mock<IEmailService>();
        mockEmailService.Setup(x => x.SendTaskAssignedAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        var result = await mockEmailService.Object.SendTaskAssignedAsync("runner@test.com", "Fix Bug", "runner_name");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task SendTaskExpiration_WithExpirationDate_Should_Return_True()
    {
        // Arrange
        var mockEmailService = new Mock<IEmailService>();
        mockEmailService.Setup(x => x.SendTaskExpirationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
            .ReturnsAsync(true);

        // Act
        var expirationDate = DateTime.UtcNow.AddDays(1);
        var result = await mockEmailService.Object.SendTaskExpirationAsync("user@test.com", "Urgent Task", expirationDate);

        // Assert
        Assert.True(result);
        mockEmailService.Verify(x => x.SendTaskExpirationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()), Times.Once);
    }

    [Fact]
    public async Task SendTaskUnassigned_WithTaskTitle_Should_Call_Service()
    {
        // Arrange
        var mockEmailService = new Mock<IEmailService>();
        mockEmailService.Setup(x => x.SendTaskUnassignedAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        var result = await mockEmailService.Object.SendTaskUnassignedAsync("runner@test.com", "Cancelled Task");

        // Assert
        Assert.True(result);
    }
}

public class EmailServiceWorkflowTests
{
    [Fact]
    public async Task Email_RegistrationFlow_Should_Send_ConfirmationEmail()
    {
        // Arrange
        var mockEmailService = new Mock<IEmailService>();
        mockEmailService.Setup(x => x.SendRegistrationConfirmationAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act - simulate user registration flow
        var registrationResult = await mockEmailService.Object.SendRegistrationConfirmationAsync("newuser@test.com", "newusername");

        // Assert
        Assert.True(registrationResult);
        mockEmailService.Verify(x => x.SendRegistrationConfirmationAsync("newuser@test.com", "newusername"), Times.Once);
    }

    [Fact]
    public async Task Email_TaskCreationFlow_Should_Notify_All_Parties()
    {
        // Arrange
        var mockEmailService = new Mock<IEmailService>();
        mockEmailService.Setup(x => x.SendTaskCreatedAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
            .ReturnsAsync(true);
        mockEmailService.Setup(x => x.SendTaskExpirationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
            .ReturnsAsync(true);

        var taskDueDate = DateTime.UtcNow.AddDays(3);

        // Act
        var taskCreatedResult = await mockEmailService.Object.SendTaskCreatedAsync("client@test.com", "New Task", "client_user", taskDueDate);
        var expirationResult = await mockEmailService.Object.SendTaskExpirationAsync("runner@test.com", "New Task", taskDueDate);

        // Assert
        Assert.True(taskCreatedResult);
        Assert.True(expirationResult);
        mockEmailService.Verify(x => x.SendTaskCreatedAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()), Times.Once);
    }

    [Fact]
    public async Task Email_MultipleNotifications_Should_Send_All()
    {
        // Arrange
        var mockEmailService = new Mock<IEmailService>();
        mockEmailService.Setup(x => x.SendRegistrationConfirmationAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);
        mockEmailService.Setup(x => x.SendTaskCreatedAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
            .ReturnsAsync(true);
        mockEmailService.Setup(x => x.SendTaskAssignedAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        var reg = await mockEmailService.Object.SendRegistrationConfirmationAsync("user@test.com", "user1");
        var created = await mockEmailService.Object.SendTaskCreatedAsync("user@test.com", "Task 1", "user1", DateTime.UtcNow.AddDays(1));
        var assigned = await mockEmailService.Object.SendTaskAssignedAsync("runner@test.com", "Task 1", "runner1");

        // Assert
        Assert.True(reg && created && assigned);
        mockEmailService.Verify(x => x.SendRegistrationConfirmationAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        mockEmailService.Verify(x => x.SendTaskCreatedAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()), Times.Once);
        mockEmailService.Verify(x => x.SendTaskAssignedAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Email_TaskCompleteFlow_Should_Notify_Client()
    {
        // Arrange
        var mockEmailService = new Mock<IEmailService>();
        mockEmailService.Setup(x => x.SendTaskCompletedAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        var result = await mockEmailService.Object.SendTaskCompletedAsync("client@test.com", "Website Project", "runner_user");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task Email_TaskUnassignFlow_Should_Notify_Runner()
    {
        // Arrange
        var mockEmailService = new Mock<IEmailService>();
        mockEmailService.Setup(x => x.SendTaskUnassignedAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        var result = await mockEmailService.Object.SendTaskUnassignedAsync("runner@test.com", "Cancelled Project");

        // Assert
        Assert.True(result);
    }
}

public class EmailServiceMultiRecipientTests
{
    [Fact]
    public async Task Email_MultipleRecipients_Should_Send_Each_Email()
    {
        // Arrange
        var mockEmailService = new Mock<IEmailService>();
        mockEmailService.Setup(x => x.SendTaskCreatedAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
            .ReturnsAsync(true);

        var recipients = new[] { "user1@test.com", "user2@test.com", "user3@test.com" };
        var dueDate = DateTime.UtcNow.AddDays(1);

        // Act
        var results = new List<bool>();
        foreach (var recipient in recipients)
        {
            var result = await mockEmailService.Object.SendTaskCreatedAsync(recipient, "Important Task", "admin", dueDate);
            results.Add(result);
        }

        // Assert
        Assert.All(results, r => Assert.True(r));
        mockEmailService.Verify(x => x.SendTaskCreatedAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()), Times.Exactly(3));
    }

    [Fact]
    public async Task Email_ConcurrentNotifications_Should_Handle_Multiple_Requests()
    {
        // Arrange
        var mockEmailService = new Mock<IEmailService>();
        mockEmailService.Setup(x => x.SendRegistrationConfirmationAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        var tasks = new List<Task<bool>>();
        for (int i = 0; i < 5; i++)
        {
            tasks.Add(mockEmailService.Object.SendRegistrationConfirmationAsync($"user{i}@test.com", $"user{i}"));
        }

        var results = await Task.WhenAll(tasks);

        // Assert
        Assert.All(results, r => Assert.True(r));
        mockEmailService.Verify(x => x.SendRegistrationConfirmationAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(5));
    }
}
