using FluentResults;
using Microsoft.EntityFrameworkCore;
using Moq;
using Tasks.Api.Data;
using Tasks.Api.Errors;
using Tasks.Api.Models;
using Tasks.Api.Services;
using Xunit;

namespace Tasks.Api.Tests.unit {
    public class UpdateTaskTests {
        private TaskContext GetInMemoryDbContext() {
            var options = new DbContextOptionsBuilder<TaskContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new TaskContext(options);
        }

        [Fact]
        public async Task UpdateTaskDetails_WhenProjectNotFound_ReturnsNotFoundError() {
            // Arrange
            var projectId = Guid.NewGuid();
            var taskId = Guid.NewGuid();
            var requestDto = new TaskItemRequestDTO { title = "Updated Task" };

            var mockProjectClient = new Mock<IProjectClient>();
            mockProjectClient
                .Setup(c => c.GetProjectByIdAsync(projectId))
                .ReturnsAsync(Result.Fail(new NotFoundError("Project not found")));

            using var context = GetInMemoryDbContext();
            var service = new TaskService(mockProjectClient.Object, context);

            // Act
            var result = await service.UpdateTaskDetails(projectId, taskId, requestDto);

            // Assert
            Assert.True(result.IsFailed);
            Assert.IsType<NotFoundError>(result.Errors.First());
        }

        [Fact]
        public async Task UpdateTaskDetails_WhenProjectsApiUnavailable_ReturnsBadGatewayError() {
            // Arrange
            var projectId = Guid.NewGuid();
            var taskId = Guid.NewGuid();
            var requestDto = new TaskItemRequestDTO { title = "Updated Task" };

            var mockProjectClient = new Mock<IProjectClient>();
            mockProjectClient
                .Setup(c => c.GetProjectByIdAsync(projectId))
                .ReturnsAsync(Result.Fail(new BadGatewayError("Projects API is unavailable")));

            using var context = GetInMemoryDbContext();
            var service = new TaskService(mockProjectClient.Object, context);

            // Act
            var result = await service.UpdateTaskDetails(projectId, taskId, requestDto);

            // Assert
            Assert.True(result.IsFailed);
            Assert.IsType<BadGatewayError>(result.Errors.First());
        }

        [Fact]
        public async Task UpdateTaskDetails_WhenTaskDoesNotExist_ReturnsNotFoundError() {
            // Arrange
            var projectId = Guid.NewGuid();
            var taskId = Guid.NewGuid();
            var requestDto = new TaskItemRequestDTO { title = "Updated Task" };
            var activeProject = new ProjectDTO(projectId, "title", false);

            var mockProjectClient = new Mock<IProjectClient>();
            mockProjectClient
                .Setup(c => c.GetProjectByIdAsync(projectId))
                .ReturnsAsync(Result.Ok(activeProject));

            using var context = GetInMemoryDbContext();
            var service = new TaskService(mockProjectClient.Object, context);

            // Act
            var result = await service.UpdateTaskDetails(projectId, taskId, requestDto);

            // Assert
            Assert.True(result.IsFailed);
            Assert.IsType<NotFoundError>(result.Errors.First());
            Assert.Equal("No task with specified id", result.Errors.First().Message);
        }

        [Fact]
        public async Task UpdateTaskDetails_WhenTaskExistsAndProjectIsValid_UpdatesTaskAndReturnsOk() {
            // Arrange
            var projectId = Guid.NewGuid();
            var activeProject = new ProjectDTO(projectId, "title", false);

            var mockProjectClient = new Mock<IProjectClient>();
            mockProjectClient
                .Setup(c => c.GetProjectByIdAsync(projectId))
                .ReturnsAsync(Result.Ok(activeProject));

            using var context = GetInMemoryDbContext();

            var task = new TaskItem(projectId, "Old Title", "Old Description", "Old Assignee", null);
            var taskId = task.id;
            var originalCreatedAt = task.createdAt;

            await context.AddAsync(task);
            await context.SaveChangesAsync();

            var service = new TaskService(mockProjectClient.Object, context);

            var requestDto = new TaskItemRequestDTO {
                title = "Updated Title",
                description = "Updated Description",
                assignee = "Updated Assignee",
                dueDate = DateTimeOffset.UtcNow.AddDays(5)
            };

            // Act
            var result = await service.UpdateTaskDetails(projectId, taskId, requestDto);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(taskId, result.Value.id);
            Assert.Equal("Updated Title", result.Value.title);
            Assert.Equal("Updated Description", result.Value.description);
            Assert.Equal("Updated Assignee", result.Value.assignee);
            Assert.NotNull(result.Value.dueDate);
            Assert.Equal(originalCreatedAt, result.Value.createdAt);
            Assert.True(result.Value.updatedAt > originalCreatedAt);
        }

        [Fact]
        public async Task UpdateTaskDetails_WhenUpdatingOnlyTitle_PreservesOtherFields() {
            // Arrange
            var projectId = Guid.NewGuid();
            var activeProject = new ProjectDTO(projectId, "title", false);

            var mockProjectClient = new Mock<IProjectClient>();
            mockProjectClient
                .Setup(c => c.GetProjectByIdAsync(projectId))
                .ReturnsAsync(Result.Ok(activeProject));

            using var context = GetInMemoryDbContext();

            var originalDescription = "Original Description";
            var originalAssignee = "Original Assignee";
            var originalDueDate = DateTimeOffset.UtcNow.AddDays(10);

            var task = new TaskItem(projectId, "Old Title", originalDescription, originalAssignee, originalDueDate);
            var taskId = task.id;

            await context.AddAsync(task);
            await context.SaveChangesAsync();

            var service = new TaskService(mockProjectClient.Object, context);

            var requestDto = new TaskItemRequestDTO { title = "Updated Title" };

            // Act
            var result = await service.UpdateTaskDetails(projectId, taskId, requestDto);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal("Updated Title", result.Value.title);
            Assert.Equal(originalDescription, result.Value.description);
            Assert.Equal(originalAssignee, result.Value.assignee);
            Assert.Equal(originalDueDate, result.Value.dueDate);
        }

        [Fact]
        public async Task UpdateTaskDetails_WhenUpdatingDescription_PreservesOtherFields() {
            // Arrange
            var projectId = Guid.NewGuid();
            var activeProject = new ProjectDTO(projectId, "title", false);

            var mockProjectClient = new Mock<IProjectClient>();
            mockProjectClient
                .Setup(c => c.GetProjectByIdAsync(projectId))
                .ReturnsAsync(Result.Ok(activeProject));

            using var context = GetInMemoryDbContext();

            var originalTitle = "Original Title";
            var originalAssignee = "Original Assignee";
            var originalDueDate = DateTimeOffset.UtcNow.AddDays(10);

            var task = new TaskItem(projectId, originalTitle, "Old Description", originalAssignee, originalDueDate);
            var taskId = task.id;

            await context.AddAsync(task);
            await context.SaveChangesAsync();

            var service = new TaskService(mockProjectClient.Object, context);

            var requestDto = new TaskItemRequestDTO { 
                title = "Updated Title",
                description = "Updated Description"
            };

            // Act
            var result = await service.UpdateTaskDetails(projectId, taskId, requestDto);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal("Updated Title", result.Value.title);
            Assert.Equal("Updated Description", result.Value.description);
            Assert.Equal(originalAssignee, result.Value.assignee);
            Assert.Equal(originalDueDate, result.Value.dueDate);
        }

        [Fact]
        public async Task UpdateTaskDetails_WhenUpdatingAssignee_PreservesOtherFields() {
            // Arrange
            var projectId = Guid.NewGuid();
            var activeProject = new ProjectDTO(projectId, "title", false);

            var mockProjectClient = new Mock<IProjectClient>();
            mockProjectClient
                .Setup(c => c.GetProjectByIdAsync(projectId))
                .ReturnsAsync(Result.Ok(activeProject));

            using var context = GetInMemoryDbContext();

            var originalTitle = "Original Title";
            var originalDescription = "Original Description";
            var originalDueDate = DateTimeOffset.UtcNow.AddDays(10);

            var task = new TaskItem(projectId, originalTitle, originalDescription, "Old Assignee", originalDueDate);
            var taskId = task.id;

            await context.AddAsync(task);
            await context.SaveChangesAsync();

            var service = new TaskService(mockProjectClient.Object, context);

            var requestDto = new TaskItemRequestDTO { 
                title = "Updated Title",
                assignee = "Updated Assignee"
            };

            // Act
            var result = await service.UpdateTaskDetails(projectId, taskId, requestDto);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal("Updated Title", result.Value.title);
            Assert.Equal(originalDescription, result.Value.description);
            Assert.Equal("Updated Assignee", result.Value.assignee);
            Assert.Equal(originalDueDate, result.Value.dueDate);
        }

        [Fact]
        public async Task UpdateTaskDetails_WhenUpdatingDueDate_PreservesOtherFields() {
            // Arrange
            var projectId = Guid.NewGuid();
            var activeProject = new ProjectDTO(projectId, "title", false);

            var mockProjectClient = new Mock<IProjectClient>();
            mockProjectClient
                .Setup(c => c.GetProjectByIdAsync(projectId))
                .ReturnsAsync(Result.Ok(activeProject));

            using var context = GetInMemoryDbContext();

            var originalTitle = "Original Title";
            var originalDescription = "Original Description";
            var originalAssignee = "Original Assignee";

            var task = new TaskItem(projectId, originalTitle, originalDescription, originalAssignee, null);
            var taskId = task.id;

            await context.AddAsync(task);
            await context.SaveChangesAsync();

            var service = new TaskService(mockProjectClient.Object, context);

            var newDueDate = DateTimeOffset.UtcNow.AddDays(5);
            var requestDto = new TaskItemRequestDTO { 
                title = "Updated Title",
                dueDate = newDueDate
            };

            // Act
            var result = await service.UpdateTaskDetails(projectId, taskId, requestDto);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal("Updated Title", result.Value.title);
            Assert.Equal(originalDescription, result.Value.description);
            Assert.Equal(originalAssignee, result.Value.assignee);
            Assert.Equal(newDueDate, result.Value.dueDate);
        }

        [Fact]
        public async Task UpdateTaskDetails_DoesNotChangeStatusOrProjectId() {
            // Arrange
            var projectId = Guid.NewGuid();
            var activeProject = new ProjectDTO(projectId, "title", false);

            var mockProjectClient = new Mock<IProjectClient>();
            mockProjectClient
                .Setup(c => c.GetProjectByIdAsync(projectId))
                .ReturnsAsync(Result.Ok(activeProject));

            using var context = GetInMemoryDbContext();

            var task = new TaskItem(projectId, "Old Title", "Old Description", "Old Assignee", null);
            var taskId = task.id;
            var originalStatus = task.status;
            var originalProjectId = task.projectId;

            await context.AddAsync(task);
            await context.SaveChangesAsync();

            var service = new TaskService(mockProjectClient.Object, context);

            var requestDto = new TaskItemRequestDTO {
                title = "Updated Title",
                description = "Updated Description"
            };

            // Act
            var result = await service.UpdateTaskDetails(projectId, taskId, requestDto);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(originalStatus, result.Value.status);
            Assert.Equal(originalProjectId, result.Value.projectId);
        }

        [Fact]
        public async Task UpdateTaskDetails_UpdatesTimestampInDatabase() {
            // Arrange
            var projectId = Guid.NewGuid();
            var activeProject = new ProjectDTO(projectId, "title", false);

            var mockProjectClient = new Mock<IProjectClient>();
            mockProjectClient
                .Setup(c => c.GetProjectByIdAsync(projectId))
                .ReturnsAsync(Result.Ok(activeProject));

            using var context = GetInMemoryDbContext();

            var task = new TaskItem(projectId, "Old Title", "Old Description", "Old Assignee", null);
            var taskId = task.id;
            var originalCreatedAt = task.createdAt;

            await context.AddAsync(task);
            await context.SaveChangesAsync();

            var service = new TaskService(mockProjectClient.Object, context);

            var requestDto = new TaskItemRequestDTO { title = "Updated Title" };

            // Act
            var result = await service.UpdateTaskDetails(projectId, taskId, requestDto);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(result.Value!.updatedAt >= originalCreatedAt);
            Assert.NotEqual(originalCreatedAt, result.Value.updatedAt);

            // Verify the change is saved to the database
            var refreshedTask = await context.Set<TaskItem>().FirstOrDefaultAsync(t => t.id == taskId);
            Assert.NotNull(refreshedTask);
            Assert.True(refreshedTask.updatedAt > originalCreatedAt);
        }

        [Fact]
        public async Task UpdateTaskDetails_WhenTaskBelongsToDifferentProject_ReturnsNotFoundError() {
            // Arrange
            var projectId1 = Guid.NewGuid();
            var projectId2 = Guid.NewGuid();
            var activeProject = new ProjectDTO(projectId1, "title", false);

            var mockProjectClient = new Mock<IProjectClient>();
            mockProjectClient
                .Setup(c => c.GetProjectByIdAsync(projectId1))
                .ReturnsAsync(Result.Ok(activeProject));

            using var context = GetInMemoryDbContext();

            var task = new TaskItem(projectId2, "Task Title", "Description", "Assignee", null);
            var taskId = task.id;

            await context.AddAsync(task);
            await context.SaveChangesAsync();

            var service = new TaskService(mockProjectClient.Object, context);

            var requestDto = new TaskItemRequestDTO { title = "Updated Title" };

            // Act
            var result = await service.UpdateTaskDetails(projectId1, taskId, requestDto);

            // Assert
            Assert.True(result.IsFailed);
            Assert.IsType<NotFoundError>(result.Errors.First());
        }
    }
}
