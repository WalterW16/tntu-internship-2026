using FluentResults;
using Microsoft.EntityFrameworkCore;
using Moq;
using Tasks.Api.Data;
using Tasks.Api.Errors;
using Tasks.Api.Models;
using Tasks.Api.Services;
using Xunit;

namespace Tasks.Api.Tests.unit {
    public class ChangeStatusTests {
        private TaskContext GetInMemoryDbContext() {
            var options = new DbContextOptionsBuilder<TaskContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new TaskContext(options);
        }

        // --- Тести перевірки API проектів та наявності завдання ---

        [Fact]
        public async Task ChangeTaskItemStatus_WhenProjectNotFound_ReturnsNotFoundError() {
            // Arrange
            var projectId = Guid.NewGuid();
            var taskId = Guid.NewGuid();

            var mockProjectClient = new Mock<IProjectClient>();
            mockProjectClient
                .Setup(c => c.GetProjectByIdAsync(projectId))
                .ReturnsAsync(Result.Fail(new NotFoundError("Project not found")));

            using var context = GetInMemoryDbContext();
            var service = new TaskService(mockProjectClient.Object, context);

            // Act
            var result = await service.ChangeTaskItemStatus(projectId, taskId, TaskItemStatus.InProgress);

            // Assert
            Assert.True(result.IsFailed);
            Assert.IsType<NotFoundError>(result.Errors.First());
        }

        [Fact]
        public async Task ChangeTaskItemStatus_WhenProjectsApiUnavailable_ReturnsBadGatewayError() {
            // Arrange
            var projectId = Guid.NewGuid();
            var taskId = Guid.NewGuid();

            var mockProjectClient = new Mock<IProjectClient>();
            mockProjectClient
                .Setup(c => c.GetProjectByIdAsync(projectId))
                .ReturnsAsync(Result.Fail(new BadGatewayError("Projects API is unavailable")));

            using var context = GetInMemoryDbContext();
            var service = new TaskService(mockProjectClient.Object, context);

            // Act
            var result = await service.ChangeTaskItemStatus(projectId, taskId, TaskItemStatus.InProgress);

            // Assert
            Assert.True(result.IsFailed);
            Assert.IsType<BadGatewayError>(result.Errors.First());
        }

        [Fact]
        public async Task ChangeTaskItemStatus_WhenTaskDoesNotExist_ReturnsNotFoundError() {
            // Arrange
            var projectId = Guid.NewGuid();
            var taskId = Guid.NewGuid();
            var activeProject = new ProjectDTO(projectId, "title", false);

            var mockProjectClient = new Mock<IProjectClient>();
            mockProjectClient
                .Setup(c => c.GetProjectByIdAsync(projectId))
                .ReturnsAsync(Result.Ok(activeProject));

            using var context = GetInMemoryDbContext();
            var service = new TaskService(mockProjectClient.Object, context);

            // Act
            var result = await service.ChangeTaskItemStatus(projectId, taskId, TaskItemStatus.InProgress);

            // Assert
            Assert.True(result.IsFailed);
            Assert.IsType<NotFoundError>(result.Errors.First());
            Assert.Equal("No task with specified id", result.Errors.First().Message);
        }

        // --- Тести дозволених переходів статусів (Happy Path) ---

        [Fact]
        public async Task ChangeTaskItemStatus_FromToDoToInProgress_ReturnsOkAndUpdatesTask() {
            // Arrange
            var projectId = Guid.NewGuid();
            var activeProject = new ProjectDTO(projectId, "title", false);

            var mockProjectClient = new Mock<IProjectClient>();
            mockProjectClient
                .Setup(c => c.GetProjectByIdAsync(projectId))
                .ReturnsAsync(Result.Ok(activeProject));

            using var context = GetInMemoryDbContext();

            // За замовчуванням нове завдання має статус ToDo
            var task = new TaskItem(projectId, "Title", "Description", "Assignee", null);
            var taskId = task.id;
            var originalUpdatedAt = task.updatedAt;

            await context.AddAsync(task);
            await context.SaveChangesAsync();

            var service = new TaskService(mockProjectClient.Object, context);

            // Act
            var result = await service.ChangeTaskItemStatus(projectId, taskId, TaskItemStatus.InProgress);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(TaskItemStatus.InProgress, result.Value.status);
            Assert.True(result.Value.updatedAt > originalUpdatedAt); // Перевірка оновлення updatedAt
        }

        [Fact]
        public async Task ChangeTaskItemStatus_FromInProgressToDone_ReturnsOkAndUpdatesTask() {
            // Arrange
            var projectId = Guid.NewGuid();
            var activeProject = new ProjectDTO(projectId, "title", false);

            var mockProjectClient = new Mock<IProjectClient>();
            mockProjectClient
                .Setup(c => c.GetProjectByIdAsync(projectId))
                .ReturnsAsync(Result.Ok(activeProject));

            using var context = GetInMemoryDbContext();
            var task = new TaskItem(projectId, "Title", "Description", "Assignee", null);
            task.setStatus(TaskItemStatus.InProgress); // Попередньо переводимо в InProgress
            var taskId = task.id;
            var originalUpdatedAt = task.updatedAt;

            await context.AddAsync(task);
            await context.SaveChangesAsync();

            var service = new TaskService(mockProjectClient.Object, context);

            // Act
            var result = await service.ChangeTaskItemStatus(projectId, taskId, TaskItemStatus.Done);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(TaskItemStatus.Done, result.Value.status);
            Assert.True(result.Value.updatedAt > originalUpdatedAt);
        }

        // --- Тести заборонених переходів статусів (Conflict) ---

        [Fact]
        public async Task ChangeTaskItemStatus_FromToDoToDone_ReturnsConflictError() {
            // Arrange
            var projectId = Guid.NewGuid();
            var activeProject = new ProjectDTO(projectId, "title", false);

            var mockProjectClient = new Mock<IProjectClient>();
            mockProjectClient
                .Setup(c => c.GetProjectByIdAsync(projectId))
                .ReturnsAsync(Result.Ok(activeProject));

            using var context = GetInMemoryDbContext();
            var task = new TaskItem(projectId, "Title", "Description", "Assignee", null); // Статус ToDo
            var taskId = task.id;

            await context.AddAsync(task);
            await context.SaveChangesAsync();

            var service = new TaskService(mockProjectClient.Object, context);

            // Act
            var result = await service.ChangeTaskItemStatus(projectId, taskId, TaskItemStatus.Done);

            // Assert
            Assert.True(result.IsFailed);
            Assert.IsType<ConflictError>(result.Errors.First());
            Assert.Contains("Can't change status", result.Errors.First().Message);
        }

        [Fact]
        public async Task ChangeTaskItemStatus_FromInProgressToToDo_ReturnsConflictError() {
            // Arrange
            var projectId = Guid.NewGuid();
            var activeProject = new ProjectDTO(projectId, "title", false);

            var mockProjectClient = new Mock<IProjectClient>();
            mockProjectClient
                .Setup(c => c.GetProjectByIdAsync(projectId))
                .ReturnsAsync(Result.Ok(activeProject));

            using var context = GetInMemoryDbContext();
            var task = new TaskItem(projectId, "Title", "Description", "Assignee", null);
            task.setStatus(TaskItemStatus.InProgress); // Статус InProgress
            var taskId = task.id;

            await context.AddAsync(task);
            await context.SaveChangesAsync();

            var service = new TaskService(mockProjectClient.Object, context);

            // Act
            var result = await service.ChangeTaskItemStatus(projectId, taskId, TaskItemStatus.ToDo);

            // Assert
            Assert.True(result.IsFailed);
            Assert.IsType<ConflictError>(result.Errors.First());
        }

        [Theory]
        [InlineData(TaskItemStatus.ToDo)]
        [InlineData(TaskItemStatus.InProgress)]
        public async Task ChangeTaskItemStatus_FromDoneToAnyStatus_ReturnsConflictError(TaskItemStatus targetStatus) {
            // Arrange
            var projectId = Guid.NewGuid();
            var activeProject = new ProjectDTO(projectId, "title", false);

            var mockProjectClient = new Mock<IProjectClient>();
            mockProjectClient
                .Setup(c => c.GetProjectByIdAsync(projectId))
                .ReturnsAsync(Result.Ok(activeProject));

            using var context = GetInMemoryDbContext();
            var task = new TaskItem(projectId, "Title", "Description", "Assignee", null);
            task.setStatus(TaskItemStatus.InProgress);
            task.setStatus(TaskItemStatus.Done); // Переводимо до кінцевого статусу Done
            var taskId = task.id;

            await context.AddAsync(task);
            await context.SaveChangesAsync();

            var service = new TaskService(mockProjectClient.Object, context);

            // Act
            var result = await service.ChangeTaskItemStatus(projectId, taskId, targetStatus);

            // Assert
            Assert.True(result.IsFailed);
            Assert.IsType<ConflictError>(result.Errors.First());
        }
    }
}