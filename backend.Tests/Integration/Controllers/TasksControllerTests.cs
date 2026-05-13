using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using backend.Tests.Helpers;
using backend.Controllers;
using Errando.Data;
using Errando.DTOs;
using Errando.Services;

namespace backend.Tests.Integration.Controllers
{
    public class TasksControllerTests : IAsyncLifetime
    {
        private AppDbContext _context = null!;
        private Mock<IEmailService> _emailServiceMock = null!;
        private Mock<IImageStorageService> _imageStorageMock = null!;
        private Mock<ILogger<TasksController>> _loggerMock = null!;
        private TasksController _controller = null!;

        public async Task InitializeAsync()
        {
            _context = TestSetup.CreateInMemoryContext();
            _emailServiceMock = new Mock<IEmailService>();
            _imageStorageMock = new Mock<IImageStorageService>();
            _loggerMock = new Mock<ILogger<TasksController>>();
            _controller = new TasksController(_context, _emailServiceMock.Object, _imageStorageMock.Object, _loggerMock.Object);
            await TestSetup.SeedTestDataAsync(_context);
        }

        public async Task DisposeAsync()
        {
            await _context.DisposeAsync();
        }

        [Fact]
        public async Task GetTasks_AsClient_ExcludesExpiredTasksAndReturnsOwnedTasks()
        {
            // Arrange
            var client = await _context.Users.FirstAsync(u => u.Role == "Client");
            var task = new TodoTask
            {
                Title = "Current Task",
                Description = "A current client task",
                ScheduledTime = DateTime.UtcNow.AddDays(1),
                Status = "Pending",
                ClientId = client.Id,
                Location = "City",
                Price = 10m,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            var expiredTask = new TodoTask
            {
                Title = "Expired Task",
                Description = "An expired client task",
                ScheduledTime = DateTime.UtcNow.AddDays(-2),
                Status = "Pending",
                ClientId = client.Id,
                ExpirationDate = DateTime.UtcNow.AddDays(-1),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Tasks.AddRange(task, expiredTask);
            await _context.SaveChangesAsync();

            var claims = TestSetup.CreateClaimsPrincipal(client.Id, client.Username, "Client");
            _controller.ControllerContext = new ControllerContext { HttpContext = TestSetup.CreateMockHttpContext(claims) };
            _emailServiceMock.Setup(x => x.SendTaskExpirationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.GetTasks();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var tasks = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);
            Assert.Single(tasks);
            _emailServiceMock.Verify(x => x.SendTaskExpirationAsync(client.Email, expiredTask.Title, expiredTask.ExpirationDate!.Value), Times.Once);
        }

        [Fact]
        public async Task GetTask_AsRunnerWithoutTaskItems_ReturnsNotFound()
        {
            // Arrange
            var runner = await _context.Users.FirstAsync(u => u.Role == "Runner");
            var client = await _context.Users.FirstAsync(u => u.Role == "Client");
            var task = new TodoTask
            {
                Title = "No Items Task",
                Description = "Task without items",
                ScheduledTime = DateTime.UtcNow.AddDays(1),
                Status = "Pending",
                ClientId = client.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            _controller.ControllerContext = new ControllerContext { HttpContext = TestSetup.CreateMockHttpContext(TestSetup.CreateClaimsPrincipal(runner.Id, runner.Username, "Runner")) };

            // Act
            var result = await _controller.GetTask(task.Id);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task CreateTask_WithInvalidRecurringData_ReturnsBadRequest()
        {
            // Arrange
            var client = await _context.Users.FirstAsync(u => u.Role == "Client");
            _controller.ControllerContext = new ControllerContext { HttpContext = TestSetup.CreateMockHttpContext(TestSetup.CreateClaimsPrincipal(client.Id, client.Username, "Client")) };

            var dto = new CreateTaskDto
            {
                Title = "Recurring",
                Description = "Invalid recurring task",
                ScheduledTime = DateTime.UtcNow.AddDays(1),
                IsRecurring = true,
                RecurringDayOfWeek = -1,
                RecurringRepetitions = 0,
                Location = "Test",
                Price = 20m
            };

            // Act
            var result = await _controller.CreateTask(dto);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Contains("Valid RecurringDayOfWeek", badRequest.Value?.ToString() ?? string.Empty);
        }

        [Fact]
        public async Task CreateTask_WithValidRecurringTask_ReturnsOkAndSendsEmails()
        {
            // Arrange
            var client = await _context.Users.FirstAsync(u => u.Role == "Client");
            var targetDayOfWeek = (int)DateTime.UtcNow.DayOfWeek;
            var scheduledTime = DateTime.UtcNow.Date.AddDays(1).AddHours(9);
            if ((int)scheduledTime.DayOfWeek != targetDayOfWeek)
            {
                scheduledTime = scheduledTime.AddDays((targetDayOfWeek - (int)scheduledTime.DayOfWeek + 7) % 7);
            }

            _controller.ControllerContext = new ControllerContext { HttpContext = TestSetup.CreateMockHttpContext(TestSetup.CreateClaimsPrincipal(client.Id, client.Username, "Client")) };
            _emailServiceMock.Setup(x => x.SendTaskCreatedAsync(client.Email, It.IsAny<string>(), client.Username, It.IsAny<DateTime>()))
                .ReturnsAsync(true);

            var dto = new CreateTaskDto
            {
                Title = "Weekly Task",
                Description = "Recurring client task",
                ScheduledTime = scheduledTime,
                IsRecurring = true,
                RecurringDayOfWeek = targetDayOfWeek,
                RecurringRepetitions = 2,
                Location = "Office",
                Price = 50m
            };

            // Act
            var result = await _controller.CreateTask(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult.Value);
            _emailServiceMock.Verify(x => x.SendTaskCreatedAsync(client.Email, It.IsAny<string>(), client.Username, It.IsAny<DateTime>()), Times.Exactly(2));
        }

        [Fact]
        public async Task DeleteTask_AsClientWithIncompleteUnpaidTask_ReturnsOk()
        {
            // Arrange
            var client = await _context.Users.FirstAsync(u => u.Role == "Client");
            var task = new TodoTask
            {
                Title = "Delete Task",
                Description = "Incomplete task pending deletion",
                ScheduledTime = DateTime.UtcNow.AddDays(1),
                Status = "Pending",
                ClientId = client.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            _controller.ControllerContext = new ControllerContext { HttpContext = TestSetup.CreateMockHttpContext(TestSetup.CreateClaimsPrincipal(client.Id, client.Username, "Client")) };

            // Act
            var result = await _controller.DeleteTask(task.Id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("Task deleted", okResult.Value?.ToString() ?? string.Empty);
            var deletedTask = await _context.Tasks.FindAsync(task.Id);
            Assert.NotNull(deletedTask);
            Assert.True(deletedTask!.IsDeletedByClient);
        }

        [Fact]
        public async Task DeleteTask_AsClientWithCompletedUnpaidTask_ReturnsBadRequest()
        {
            // Arrange
            var client = await _context.Users.FirstAsync(u => u.Role == "Client");
            var task = new TodoTask
            {
                Title = "Completed Unpaid Task",
                Description = "Completed task pending payment",
                ScheduledTime = DateTime.UtcNow.AddDays(1),
                Status = "Completed",
                ClientId = client.Id,
                TaskItems = new List<TaskItem>
                {
                    new TaskItem { Description = "Step 1", Status = "Completed", IsCompleted = true },
                    new TaskItem { Description = "Step 2", Status = "Completed", IsCompleted = true }
                },
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            _controller.ControllerContext = new ControllerContext { HttpContext = TestSetup.CreateMockHttpContext(TestSetup.CreateClaimsPrincipal(client.Id, client.Username, "Client")) };

            // Act
            var result = await _controller.DeleteTask(task.Id);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("completed tasks after payment", badRequest.Value?.ToString() ?? string.Empty);
        }

        [Fact]
        public async Task DeleteTask_AsAdminHardDeletesTask_ReturnsOk()
        {
            // Arrange
            var admin = await _context.Users.FirstAsync(u => u.Role == "Admin");
            var client = await _context.Users.FirstAsync(u => u.Role == "Client");
            var task = new TodoTask
            {
                Title = "Admin Delete Task",
                Description = "Task for deletion",
                ScheduledTime = DateTime.UtcNow.AddDays(1),
                Status = "Pending",
                ClientId = client.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            _controller.ControllerContext = new ControllerContext { HttpContext = TestSetup.CreateMockHttpContext(TestSetup.CreateClaimsPrincipal(admin.Id, admin.Username, "Admin")) };

            // Act
            var result = await _controller.DeleteTask(task.Id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("permanently deleted", okResult.Value?.ToString() ?? string.Empty);
            Assert.Null(await _context.Tasks.FindAsync(task.Id));
        }

        [Fact]
        public async Task AssignTask_WhenAlreadyAssigned_ReturnsBadRequest()
        {
            // Arrange
            var runner = await _context.Users.FirstAsync(u => u.Role == "Runner");
            var client = await _context.Users.FirstAsync(u => u.Role == "Client");
            var task = new TodoTask
            {
                Title = "Already Assigned",
                Description = "Assigned task",
                ScheduledTime = DateTime.UtcNow.AddDays(1),
                Status = "Pending",
                ClientId = client.Id,
                RunnerId = runner.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            _controller.ControllerContext = new ControllerContext { HttpContext = TestSetup.CreateMockHttpContext(TestSetup.CreateClaimsPrincipal(runner.Id, runner.Username, "Runner")) };

            // Act
            var result = await _controller.AssignTask(task.Id);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Contains("already assigned", badRequest.Value?.ToString() ?? string.Empty);
        }

        [Fact]
        public async Task UnassignTask_WithCompletedItems_ReturnsBadRequest()
        {
            // Arrange
            var runner = await _context.Users.FirstAsync(u => u.Role == "Runner");
            var client = await _context.Users.FirstAsync(u => u.Role == "Client");
            var task = new TodoTask
            {
                Title = "Task With Completed Items",
                Description = "Cannot unassign",
                ScheduledTime = DateTime.UtcNow.AddDays(1),
                Status = "InProgress",
                ClientId = client.Id,
                RunnerId = runner.Id,
                TaskItems = new List<TaskItem>
                {
                    new TaskItem { Description = "Step 1", Status = "Completed", IsCompleted = true }
                },
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            _controller.ControllerContext = new ControllerContext { HttpContext = TestSetup.CreateMockHttpContext(TestSetup.CreateClaimsPrincipal(runner.Id, runner.Username, "Runner")) };

            // Act
            var result = await _controller.UnassignTask(task.Id);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Contains("Cannot unassign task", badRequest.Value?.ToString() ?? string.Empty);
        }

        [Fact]
        public async Task UploadTaskPhoto_WithInvalidFile_ReturnsBadRequest()
        {
            // Arrange
            var client = await _context.Users.FirstAsync(u => u.Role == "Client");
            var file = new FormFile(new MemoryStream(), 0, 0, "file", "empty.png")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/png"
            };

            _controller.ControllerContext = new ControllerContext { HttpContext = TestSetup.CreateMockHttpContext(TestSetup.CreateClaimsPrincipal(client.Id, client.Username, "Client")) };

            // Act
            var result = await _controller.UploadTaskPhoto(file);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Contains("No file provided", badRequest.Value?.ToString() ?? string.Empty);
        }

        [Fact]
        public async Task UploadTaskPhoto_WithValidFile_ReturnsPhotoUrl()
        {
            // Arrange
            var client = await _context.Users.FirstAsync(u => u.Role == "Client");
            var fileContent = new byte[] { 1, 2, 3, 4 };
            using var stream = new MemoryStream(fileContent);
            var file = new FormFile(stream, 0, fileContent.Length, "file", "image.png")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/png"
            };
            _imageStorageMock.Setup(x => x.UploadImageAsync(It.IsAny<Stream>(), "image.png", "image/png"))
                .ReturnsAsync("/uploads/task-photos/image.png");

            _controller.ControllerContext = new ControllerContext { HttpContext = TestSetup.CreateMockHttpContext(TestSetup.CreateClaimsPrincipal(client.Id, client.Username, "Client")) };

            // Act
            var result = await _controller.UploadTaskPhoto(file);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var payload = okResult.Value!;
            var photoUrlProperty = payload.GetType().GetProperty("photoUrl");
            Assert.NotNull(photoUrlProperty);
            Assert.Equal("/uploads/task-photos/image.png", photoUrlProperty!.GetValue(payload));
        }

        [Fact]
        public async Task AssignTask_AsRunner_AssignsTaskAndSendsEmail()
        {
            // Arrange
            var runner = await _context.Users.FirstAsync(u => u.Role == "Runner");
            var client = await _context.Users.FirstAsync(u => u.Role == "Client");
            var task = new TodoTask
            {
                Title = "Assignable Task",
                Description = "Task to assign",
                ScheduledTime = DateTime.UtcNow.AddDays(1),
                Status = "Pending",
                ClientId = client.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            _emailServiceMock.Setup(x => x.SendTaskAssignedAsync(client.Email, task.Title, runner.Username))
                .ReturnsAsync(true);

            _controller.ControllerContext = new ControllerContext { HttpContext = TestSetup.CreateMockHttpContext(TestSetup.CreateClaimsPrincipal(runner.Id, runner.Username, "Runner")) };

            // Act
            var result = await _controller.AssignTask(task.Id);

            // Assert
            Assert.NotNull(result.Value);
            Assert.Equal(runner.Id, result.Value!.RunnerId);
            _emailServiceMock.Verify(x => x.SendTaskAssignedAsync(client.Email, task.Title, runner.Username), Times.Once);
        }
    }
}
