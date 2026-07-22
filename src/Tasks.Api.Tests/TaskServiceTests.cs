using FluentResults;
using Microsoft.EntityFrameworkCore;
using Moq;
using Tasks.Api.Data;
using Tasks.Api.Errors;
using Tasks.Api.Models;
using Tasks.Api.Services;
using Xunit;

namespace Tasks.Api.Tests{
    public class TaskServiceTests {
        private TaskContext GetInMemoryDbContext() {
            var options = new DbContextOptionsBuilder<TaskContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new TaskContext(options);
        }

        [Fact]
        public async Task CreateTaskInProjectAsync_WhenProjectNotFound_ReturnsNotFoundError() {
            // Arrange
            var projectId = Guid.NewGuid();
            var requestDto = new TaskItemRequestDTO { title = "Test Task" };

            var mockProjectClient = new Mock<IProjectClient>();
            mockProjectClient
                .Setup(c => c.GetProjectByIdAsync(projectId))
                .ReturnsAsync(Result.Fail(new NotFoundError("Project not found")));

            using var context = GetInMemoryDbContext();
            var service = new TaskService(mockProjectClient.Object, context);

            // Act
            var result = await service.CreateTaskInProjectAsync(projectId, requestDto);

            // Assert
            Assert.True(result.IsFailed);
            Assert.IsType<NotFoundError>(result.Errors.First());
        }

        [Fact]
        public async Task CreateTaskInProjectAsync_WhenProjectsApiUnavailable_ReturnsBadGatewayError() {
            // Arrange
            var projectId = Guid.NewGuid();
            var requestDto = new TaskItemRequestDTO { title = "Test Task" };

            var mockProjectClient = new Mock<IProjectClient>();
            mockProjectClient
                .Setup(c => c.GetProjectByIdAsync(projectId))
                .ReturnsAsync(Result.Fail(new BadGatewayError("Projects API is unavailable.")));

            using var context = GetInMemoryDbContext();
            var service = new TaskService(mockProjectClient.Object, context);

            // Act
            var result = await service.CreateTaskInProjectAsync(projectId, requestDto);

            // Assert
            Assert.True(result.IsFailed);
            Assert.IsType<BadGatewayError>(result.Errors.First());
        }

        [Fact]
        public async Task CreateTaskInProjectAsync_WhenProjectIsArchived_ReturnsConflictError() {
            // Arrange
            var projectId = Guid.NewGuid();
            var requestDto = new TaskItemRequestDTO { title = "Test Task" };
            var archivedProject = new ProjectDTO (projectId, "title", true );

            var mockProjectClient = new Mock<IProjectClient>();
            mockProjectClient
                .Setup(c => c.GetProjectByIdAsync(projectId))
                .ReturnsAsync(Result.Ok(archivedProject));

            using var context = GetInMemoryDbContext();
            var service = new TaskService(mockProjectClient.Object, context);

            // Act
            var result = await service.CreateTaskInProjectAsync(projectId, requestDto);

            // Assert
            Assert.True(result.IsFailed);
            Assert.IsType<ConflictError>(result.Errors.First());
            Assert.Equal("Can't create task in archived project", result.Errors.First().Message);
        }

        [Fact]
        public async Task CreateTaskInProjectAsync_WhenProjectIsValid_CreatesTaskAndReturnsOk() {
            // Arrange
            var projectId = Guid.NewGuid();
            var requestDto = new TaskItemRequestDTO {
                title = "New Feature",
                description = "Implement tests",
                assignee = "John Doe"
            };
            var activeProject = new ProjectDTO (projectId, "title", false);

            var mockProjectClient = new Mock<IProjectClient>();
            mockProjectClient
                .Setup(c => c.GetProjectByIdAsync(projectId))
                .ReturnsAsync(Result.Ok(activeProject));

            using var context = GetInMemoryDbContext();
            var service = new TaskService(mockProjectClient.Object, context);

            // Act
            var result = await service.CreateTaskInProjectAsync(projectId, requestDto);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            Assert.Equal(requestDto.title, result.Value.title); 

            var savedTasksCount = await context.Set<TaskItem>().CountAsync(); 
            Assert.Equal(1, savedTasksCount);
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
            var service = new TaskService(mockProjectClient.Object, context);

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
            var service = new TaskService(mockProjectClient.Object, context);

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
            var service = new TaskService(mockProjectClient.Object, context);

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

            var service = new TaskService(mockProjectClient.Object, context);

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
        [Fact]
        public async Task GetTaskByIdInProjectAsync_WhenProjectNotFound_ReturnsNotFoundError() {
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
            var result = await service.GetTaskByIdInProjectAsync(projectId, taskId);

            // Assert
            Assert.True(result.IsFailed);
            Assert.IsType<NotFoundError>(result.Errors.First());
        }

        [Fact]
        public async Task GetTaskByIdInProjectAsync_WhenProjectsApiUnavailable_ReturnsBadGatewayError() {
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
            var result = await service.GetTaskByIdInProjectAsync(projectId, taskId);

            // Assert
            Assert.True(result.IsFailed);
            Assert.IsType<BadGatewayError>(result.Errors.First());
        }

        [Fact]
        public async Task GetTaskByIdInProjectAsync_WhenProjectIsValidButTaskDoesNotExist_ReturnsNotFoundError() {
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
            var result = await service.GetTaskByIdInProjectAsync(projectId, taskId);

            // Assert
            Assert.True(result.IsFailed);
            Assert.IsType<NotFoundError>(result.Errors.First());
            Assert.Equal("No task with specified id", result.Errors.First().Message);
        }

        [Fact]
        public async Task GetTaskByIdInProjectAsync_WhenTaskExists_ReturnsOkWithTask() {
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
            var result = await service.GetTaskByIdInProjectAsync(projectId, taskId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(taskId, result.Value.id);
            Assert.Equal("Target Task", result.Value.title);
        }
    }
}