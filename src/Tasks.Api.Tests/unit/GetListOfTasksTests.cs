using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Tasks.Api.Data;
using Tasks.Api.Errors;
using Tasks.Api.Models;
using Tasks.Api.Services;
using Xunit;

namespace Tasks.Api.Tests.unit {
    public class GetListOfTasksTests {
        private TaskContext GetInMemoryDbContext() {
            var options = new DbContextOptionsBuilder<TaskContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new TaskContext(options);
        }

        [Fact]
        public async Task GetListOfTasksForProjectAsync_WhenProjectNotFound_ReturnsNotFoundError() {
            // Arrange
            var projectId = Guid.NewGuid();
            var mockProjectClient = new Mock<IProjectClient>();
            mockProjectClient
                .Setup(c => c.GetProjectByIdAsync(projectId))
                .ReturnsAsync(Result.Fail(new NotFoundError("Project not found")));

            using var context = GetInMemoryDbContext();
            var service = new TaskService(mockProjectClient.Object, context, NullLogger<TaskService>.Instance);

            // Act
            var result = await service.GetListOfTasksForProjectAsync(projectId);

            // Assert
            Assert.True(result.IsFailed);
            Assert.IsType<NotFoundError>(result.Errors.First());
        }

        [Fact]
        public async Task GetListOfTasksForProjectAsync_WhenProjectsApiUnavailable_ReturnsBadGatewayError() {
            // Arrange
            var projectId = Guid.NewGuid();
            var mockProjectClient = new Mock<IProjectClient>();
            mockProjectClient
                .Setup(c => c.GetProjectByIdAsync(projectId))
                .ReturnsAsync(Result.Fail(new BadGatewayError("Projects API is unavailable.")));

            using var context = GetInMemoryDbContext();
            var service = new TaskService(mockProjectClient.Object, context, NullLogger<TaskService>.Instance);

            // Act
            var result = await service.GetListOfTasksForProjectAsync(projectId);

            // Assert
            Assert.True(result.IsFailed);
            Assert.IsType<BadGatewayError>(result.Errors.First());
        }

        [Fact]
        public async Task GetListOfTasksForProjectAsync_WhenProjectHasNoTasks_ReturnsEmptyList() {
            // Arrange
            var projectId = Guid.NewGuid();
            var activeProject = new ProjectDTO(projectId, "title", false);

            var mockProjectClient = new Mock<IProjectClient>();
            mockProjectClient
                .Setup(c => c.GetProjectByIdAsync(projectId))
                .ReturnsAsync(Result.Ok(activeProject));

            using var context = GetInMemoryDbContext();
            var service = new TaskService(mockProjectClient.Object, context, NullLogger<TaskService>.Instance);

            // Act
            var result = await service.GetListOfTasksForProjectAsync(projectId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Empty(result.Value);
        }

        [Fact]
        public async Task GetListOfTasksForProjectAsync_WhenProjectIsValid_ReturnsTasksOrderedByCreatedAtDescending() {
            // Arrange
            var targetProjectId = Guid.NewGuid();
            var otherProjectId = Guid.NewGuid();
            var activeProject = new ProjectDTO(targetProjectId, "title", false);

            var mockProjectClient = new Mock<IProjectClient>();
            mockProjectClient
                .Setup(c => c.GetProjectByIdAsync(targetProjectId))
                .ReturnsAsync(Result.Ok(activeProject));

            using var context = GetInMemoryDbContext();

            var olderTask = new TaskItem(targetProjectId, "Older Task", "Desc", "Assignee", DateTime.UtcNow.AddDays(1));
            var newerTask = new TaskItem(targetProjectId, "Newer Task", "Desc", "Assignee", DateTime.UtcNow.AddDays(2));

            var otherProjectTask = new TaskItem(otherProjectId, "Other Project Task", "Desc", "Assignee", DateTime.UtcNow);

            await context.AddRangeAsync(olderTask, newerTask, otherProjectTask);
            await context.SaveChangesAsync();

            var service = new TaskService(mockProjectClient.Object, context, NullLogger<TaskService>.Instance);

            // Act
            var result = await service.GetListOfTasksForProjectAsync(targetProjectId);

            // Assert
            Assert.True(result.IsSuccess);

            var returnedTasks = result.Value;

            Assert.Equal(2, returnedTasks.Count);
            Assert.DoesNotContain(returnedTasks, t => t.projectId == otherProjectId);

            Assert.Equal(newerTask.title, returnedTasks[0].title);
            Assert.Equal(olderTask.title, returnedTasks[1].title);
        }
              
        [Theory]
        [InlineData(TaskItemStatus.ToDo)]
        [InlineData(TaskItemStatus.InProgress)]
        [InlineData(TaskItemStatus.Done)]
        public async Task GetListOfTasksForProjectAsync_WithStatusFilter_ReturnsOnlyMatchingTasks(TaskItemStatus requestedStatus) {
            // Arrange
            var targetProjectId = Guid.NewGuid();
            var activeProject = new ProjectDTO(targetProjectId, "title", false);

            var mockProjectClient = new Mock<IProjectClient>();
            mockProjectClient
                .Setup(c => c.GetProjectByIdAsync(targetProjectId))
                .ReturnsAsync(Result.Ok(activeProject));

            using var context = GetInMemoryDbContext();

            // Створюємо по одній тасці кожного статусу
            var taskToDo = new TaskItem(targetProjectId, "Task 1", "Desc", "Assignee", DateTime.UtcNow);
            taskToDo.SetStatus(TaskItemStatus.ToDo);
            var taskInProgress = new TaskItem(targetProjectId, "Task 2", "Desc", "Assignee", DateTime.UtcNow);
            taskInProgress.SetStatus(TaskItemStatus.InProgress);
            var taskDone = new TaskItem(targetProjectId, "Task 3", "Desc", "Assignee", DateTime.UtcNow);
            taskDone.SetStatus(TaskItemStatus.InProgress);
            taskDone.SetStatus(TaskItemStatus.Done);

            await context.AddRangeAsync(taskToDo, taskInProgress, taskDone);
            await context.SaveChangesAsync();

            var service = new TaskService(mockProjectClient.Object, context, NullLogger<TaskService>.Instance);

            // Act
            // Передаємо requestedStatus у метод сервісу
            var result = await service.FilterByStatusAsync(targetProjectId, requestedStatus);

            // Assert
            Assert.True(result.IsSuccess);

            var returnedTasks = result.Value;

            // Має повернутися рівно одна таска
            Assert.Single(returnedTasks);
            // Її статус має чітко збігатися з тим, що ми запитували
            Assert.Equal(requestedStatus, returnedTasks.First().status);
        }
    }
}