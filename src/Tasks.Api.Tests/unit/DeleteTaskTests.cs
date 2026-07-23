using FluentResults;
using Microsoft.EntityFrameworkCore;
using Moq;
using Tasks.Api.Data;
using Tasks.Api.Errors;
using Tasks.Api.Models;
using Tasks.Api.Services;
using Xunit;


namespace Tasks.Api.Tests.unit {
    public  class DeleteTaskTests {
        private TaskContext GetInMemoryDbContext() {
            var options = new DbContextOptionsBuilder<TaskContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new TaskContext(options);
        }
        [Fact]
        public async Task DeleteTaskAsync_WhenProjectNotFound_ReturnsNotFoundError() {
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
            var result = await service.DeleteTaskAsync(projectId, taskId);

            // Assert
            Assert.True(result.IsFailed);
            Assert.IsType<NotFoundError>(result.Errors.First());
        }

        [Fact]
        public async Task DeleteTaskAsync_WhenProjectsApiUnavailable_ReturnsBadGatewayError() {
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
            var result = await service.DeleteTaskAsync(projectId, taskId);

            // Assert
            Assert.True(result.IsFailed);
            Assert.IsType<BadGatewayError>(result.Errors.First());
        }

        [Fact]
        public async Task DeleteTaskAsync_WhenProjectIsValidButTaskDoesNotExist_ReturnsNotFoundError() {
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
            var result = await service.DeleteTaskAsync(projectId, taskId);

            // Assert
            Assert.True(result.IsFailed);
            Assert.IsType<NotFoundError>(result.Errors.First());
            Assert.Equal("No task with specified id", result.Errors.First().Message);
        }
        [Fact]
        public async Task DeleteTaskAsync_WhenTaskExists_ReturnsOkWithTask() {
            // Arrange
            var projectId = Guid.NewGuid();
            var activeProject = new ProjectDTO(projectId, "title", false);

            var mockProjectClient = new Mock<IProjectClient>();
            mockProjectClient
                .Setup(c => c.GetProjectByIdAsync(projectId))
                .ReturnsAsync(Result.Ok(activeProject));

            using var context = GetInMemoryDbContext();

            var task = new TaskItem(projectId, "Target Task", "Desc", "Assignee", DateTime.UtcNow);
            var taskId = task.id;
            var otherTask = new TaskItem(projectId, "Other Task", "Desc", "Assignee", DateTime.UtcNow);

            await context.AddRangeAsync(task, otherTask);
            await context.SaveChangesAsync();

            var service = new TaskService(mockProjectClient.Object, context);

            // Act
            var result = await service.DeleteTaskAsync(projectId, taskId);

            // Assert
            Assert.True(result.IsSuccess);
        }
    }
}
