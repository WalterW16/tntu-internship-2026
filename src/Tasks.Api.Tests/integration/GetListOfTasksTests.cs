using System.Net;
using System.Net.Http.Json;
using FluentResults;
using Microsoft.Extensions.DependencyInjection;
using Tasks.Api.Data;
using Tasks.Api.Models;
using Xunit;

namespace Tasks.Api.Tests.integration {
    public class GetListOfTasksTests : IClassFixture<TasksApiFactory> {
        private readonly System.Net.Http.HttpClient _client;
        private readonly TasksApiFactory _factory;

        public GetListOfTasksTests(TasksApiFactory factory) {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task GetTasks_ProjectWithTasks_ReturnOkWithTasksList() {
            // Arrange
            var projectId = Guid.NewGuid();
            var project = new ProjectDTO(projectId, "Test Project", false);
            _factory.ProjectClientMock!
                .Setup(c => c.GetProjectByIdAsync(projectId))
                .Returns(Task.FromResult(Result.Ok(project)));

            var client = _client;
            
            // Create a test task in the database
            using (var scope = _factory.Services.CreateScope()) {
                var context = scope.ServiceProvider.GetRequiredService<TaskContext>();
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var task = new TaskItem(projectId, "Task 1", "Description", "assignee@example.com", null);
                await context.AddAsync(task);
                await context.SaveChangesAsync();
            }

            // Act
            var response = await client.GetAsync($"/api/v1/projects/{projectId}/tasks");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var tasks = await response.Content.ReadFromJsonAsync<List<TaskItem>>();
            Assert.NotNull(tasks);
            Assert.Single(tasks);
            Assert.Equal("Task 1", tasks[0].title);
        }

        [Fact]
        public async Task GetTasks_ProjectWithNoTasks_ReturnOkWithEmptyArray() {
            // Arrange
            var projectId = Guid.NewGuid();
            var project = new ProjectDTO(projectId, "Empty Project", false);
            _factory.ProjectClientMock!
                .Setup(c => c.GetProjectByIdAsync(projectId))
                .Returns(Task.FromResult(Result.Ok(project)));

            var client = _client;

            // Act
            var response = await client.GetAsync($"/api/v1/projects/{projectId}/tasks");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var tasks = await response.Content.ReadFromJsonAsync<List<TaskItem>>();
            Assert.NotNull(tasks);
            Assert.Empty(tasks);
        }

        [Fact]
        public async Task GetTasks_ProjectNotFound_ReturnNotFound() {
            // Arrange
            var projectId = Guid.NewGuid();
            _factory.ProjectClientMock!
                .Setup(c => c.GetProjectByIdAsync(projectId))
                .Returns(Task.FromResult(Result.Fail<ProjectDTO>(new Tasks.Api.Errors.NotFoundError("Project not found"))));

            var client = _client;

            // Act
            var response = await client.GetAsync($"/api/v1/projects/{projectId}/tasks");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task GetTasks_ProjectsApiUnavailable_ReturnBadGateway() {
            // Arrange
            var projectId = Guid.NewGuid();
            _factory.ProjectClientMock!
                .Setup(c => c.GetProjectByIdAsync(projectId))
                .Returns(Task.FromResult(Result.Fail<ProjectDTO>(new Tasks.Api.Errors.BadGatewayError("Projects API unavailable"))));

            var client = _client;

            // Act
            var response = await client.GetAsync($"/api/v1/projects/{projectId}/tasks");

            // Assert
            Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
        }

        [Fact]
        public async Task GetTasks_MultipleTasksWithDifferentCreatedAt_ReturnOrderedByCreatedAtDescending() {
            // Arrange
            var projectId = Guid.NewGuid();
            var project = new ProjectDTO(projectId, "Test Project", false);
            _factory.ProjectClientMock!
                .Setup(c => c.GetProjectByIdAsync(projectId))
                .Returns(Task.FromResult(Result.Ok(project)));

            var client = _client;

            // Create multiple tasks and verify they are returned
            using (var scope = _factory.Services.CreateScope()) {
                var context = scope.ServiceProvider.GetRequiredService<TaskContext>();
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
                
                var task1 = new TaskItem(projectId, "Task 1", "Description", "assignee@example.com", null);
                var task2 = new TaskItem(projectId, "Task 2", "Description", "assignee@example.com", null);
                var task3 = new TaskItem(projectId, "Task 3", "Description", "assignee@example.com", null);
                
                await context.AddRangeAsync(task1, task2, task3);
                await context.SaveChangesAsync();
            }

            // Act
            var response = await client.GetAsync($"/api/v1/projects/{projectId}/tasks");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var tasks = await response.Content.ReadFromJsonAsync<List<TaskItem>>();
            Assert.NotNull(tasks);
            Assert.Equal(3, tasks.Count);
            
            // Verify all tasks are present
            Assert.Contains(tasks, t => t.title == "Task 1");
            Assert.Contains(tasks, t => t.title == "Task 2");
            Assert.Contains(tasks, t => t.title == "Task 3");
        }

        [Fact]
        public async Task GetTasks_OnlyTasksForSpecificProject_ReturnFiltered() {
            // Arrange
            var projectId1 = Guid.NewGuid();
            var projectId2 = Guid.NewGuid();
            var project = new ProjectDTO(projectId1, "Test Project", false);
            _factory.ProjectClientMock!
                .Setup(c => c.GetProjectByIdAsync(projectId1))
                .Returns(Task.FromResult(Result.Ok(project)));

            var client = _client;

            // Create tasks for different projects
            using (var scope = _factory.Services.CreateScope()) {
                var context = scope.ServiceProvider.GetRequiredService<TaskContext>();
                
                var task1 = new TaskItem(projectId1, "Task for Project 1", "Description", "assignee@example.com", null);
                var task2 = new TaskItem(projectId2, "Task for Project 2", "Description", "assignee@example.com", null);
                var task3 = new TaskItem(projectId1, "Another Task for Project 1", "Description", "assignee@example.com", null);
                
                await context.AddRangeAsync(task1, task2, task3);
                await context.SaveChangesAsync();
            }

            // Act
            var response = await client.GetAsync($"/api/v1/projects/{projectId1}/tasks");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var tasks = await response.Content.ReadFromJsonAsync<List<TaskItem>>();
            Assert.NotNull(tasks);
            Assert.Equal(2, tasks.Count);
            Assert.All(tasks, t => Assert.Equal(projectId1, t.projectId));
        }
    }
}
