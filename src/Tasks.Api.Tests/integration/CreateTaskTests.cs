using System.Net;
using System.Net.Http.Json;
using FluentResults;
using Microsoft.Extensions.DependencyInjection;
using Tasks.Api.Data;
using Tasks.Api.Models;
using Xunit;

namespace Tasks.Api.Tests.integration {
    public class CreateTaskTests : IClassFixture<TasksApiFactory> {
        private readonly System.Net.Http.HttpClient _client;
        private readonly TasksApiFactory _factory;

        public CreateTaskTests(TasksApiFactory factory) {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task PostTask_ValidDataWithAllFields_ReturnCreatedWithTaskInToDoStatus() {
            // Arrange
            var projectId = Guid.NewGuid();
            var project = new ProjectDTO(projectId, "Test Project", false);
            
            _factory.ProjectClientMock!
                .Setup(c => c.GetProjectByIdAsync(projectId))
                .Returns(Task.FromResult(Result.Ok(project)));

            var client = _client;
            var requestDto = new TaskItemRequestDTO {
                title = "New Feature",
                description = "Implement authentication",
                assignee = "john@example.com",
                dueDate = DateTimeOffset.UtcNow.AddDays(5)
            };

            // Act
            var response = await client.PostAsJsonAsync($"/api/v1/projects/{projectId}/tasks", requestDto);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var createdTask = await response.Content.ReadFromJsonAsync<TaskItem>();
            Assert.NotNull(createdTask);
            Assert.Equal("New Feature", createdTask.title);
            Assert.Equal("Implement authentication", createdTask.description);
            Assert.Equal("john@example.com", createdTask.assignee);
            Assert.Equal("ToDo", createdTask.status.ToString());
            Assert.Equal(projectId, createdTask.projectId);
        }

        [Fact]
        public async Task PostTask_ValidDataWithoutOptionalFields_ReturnCreatedWithNullValues() {
            // Arrange
            var projectId = Guid.NewGuid();
            var project = new ProjectDTO(projectId, "Test Project", false);
            _factory.ProjectClientMock!
                .Setup(c => c.GetProjectByIdAsync(projectId))
                .Returns(Task.FromResult(Result.Ok(project)));

            var client = _client;
            var requestDto = new TaskItemRequestDTO { title = "Minimal Task" };

            // Act
            var response = await client.PostAsJsonAsync($"/api/v1/projects/{projectId}/tasks", requestDto);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var createdTask = await response.Content.ReadFromJsonAsync<TaskItem>();
            Assert.NotNull(createdTask);
            Assert.Equal("Minimal Task", createdTask.title);
            Assert.Null(createdTask.description);
            Assert.Null(createdTask.assignee);
            Assert.Null(createdTask.dueDate);
            Assert.Equal("ToDo", createdTask.status.ToString());
        }

        [Fact]
        public async Task PostTask_ProjectNotFound_ReturnNotFound() {
            // Arrange
            var projectId = Guid.NewGuid();
            _factory.ProjectClientMock!
                .Setup(c => c.GetProjectByIdAsync(projectId))
                .Returns(Task.FromResult(Result.Fail<ProjectDTO>(new Tasks.Api.Errors.NotFoundError("Project not found"))));

            var client = _client;
            var requestDto = new TaskItemRequestDTO { title = "Test Task" };

            // Act
            var response = await client.PostAsJsonAsync($"/api/v1/projects/{projectId}/tasks", requestDto);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task PostTask_ProjectIsArchived_ReturnConflict() {
            // Arrange
            var projectId = Guid.NewGuid();
            var project = new ProjectDTO(projectId, "Archived Project", true);
            _factory.ProjectClientMock!
                .Setup(c => c.GetProjectByIdAsync(projectId))
                .Returns(Task.FromResult(Result.Ok(project)));

            var client = _client;
            var requestDto = new TaskItemRequestDTO { title = "Test Task" };

            // Act
            var response = await client.PostAsJsonAsync($"/api/v1/projects/{projectId}/tasks", requestDto);

            // Assert
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task PostTask_ProjectsApiUnavailable_ReturnBadGateway() {
            // Arrange
            var projectId = Guid.NewGuid();
            _factory.ProjectClientMock!
                .Setup(c => c.GetProjectByIdAsync(projectId))
                .Returns(Task.FromResult(Result.Fail<ProjectDTO>(new Tasks.Api.Errors.BadGatewayError("Projects API unavailable"))));

            var client = _client;
            var requestDto = new TaskItemRequestDTO { title = "Test Task" };

            // Act
            var response = await client.PostAsJsonAsync($"/api/v1/projects/{projectId}/tasks", requestDto);

            // Assert
            Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
        }

        [Fact]
        public async Task PostTask_EmptyTitle_ReturnBadRequest() {
            // Arrange
            var projectId = Guid.NewGuid();
            var project = new ProjectDTO(projectId, "Test Project", false);
            _factory.ProjectClientMock!
                .Setup(c => c.GetProjectByIdAsync(projectId))
                .Returns(Task.FromResult(Result.Ok(project)));

            var client = _client;
            var requestDto = new TaskItemRequestDTO { title = "" };

            // Act
            var response = await client.PostAsJsonAsync($"/api/v1/projects/{projectId}/tasks", requestDto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PostTask_WhitespaceTitle_ReturnBadRequest() {
            // Arrange
            var projectId = Guid.NewGuid();
            var project = new ProjectDTO(projectId, "Test Project", false);
            _factory.ProjectClientMock!
                .Setup(c => c.GetProjectByIdAsync(projectId))
                .Returns(Task.FromResult(Result.Ok(project)));

            var client = _client;
            var requestDto = new TaskItemRequestDTO { title = "   " };

            // Act
            var response = await client.PostAsJsonAsync($"/api/v1/projects/{projectId}/tasks", requestDto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PostTask_TitleTooLong_ReturnBadRequest() {
            // Arrange
            var projectId = Guid.NewGuid();
            var project = new ProjectDTO(projectId, "Test Project", false);
            _factory.ProjectClientMock!
                .Setup(c => c.GetProjectByIdAsync(projectId))
                .Returns(Task.FromResult(Result.Ok(project)));

            var client = _client;
            var longTitle = new string('a', 201);
            var requestDto = new TaskItemRequestDTO { title = longTitle };

            // Act
            var response = await client.PostAsJsonAsync($"/api/v1/projects/{projectId}/tasks", requestDto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PostTask_DescriptionTooLong_ReturnBadRequest() {
            // Arrange
            var projectId = Guid.NewGuid();
            var project = new ProjectDTO(projectId, "Test Project", false);
            _factory.ProjectClientMock!
                .Setup(c => c.GetProjectByIdAsync(projectId))
                .Returns(Task.FromResult(Result.Ok(project)));

            var client = _client;
            var longDescription = new string('a', 2001);
            var requestDto = new TaskItemRequestDTO { 
                title = "Valid Title",
                description = longDescription
            };

            // Act
            var response = await client.PostAsJsonAsync($"/api/v1/projects/{projectId}/tasks", requestDto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PostTask_AssigneeTooLong_ReturnBadRequest() {
            // Arrange
            var projectId = Guid.NewGuid();
            var project = new ProjectDTO(projectId, "Test Project", false);
            _factory.ProjectClientMock!
                .Setup(c => c.GetProjectByIdAsync(projectId))
                .Returns(Task.FromResult(Result.Ok(project)));

            var client = _client;
            var longAssignee = new string('a', 201);
            var requestDto = new TaskItemRequestDTO { 
                title = "Valid Title",
                assignee = longAssignee
            };

            // Act
            var response = await client.PostAsJsonAsync($"/api/v1/projects/{projectId}/tasks", requestDto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
