using System;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Tasks.Api.Data;
using Tasks.Api.Errors;
using Tasks.Api.Models;
using Tasks.Api.Services;
using Xunit;

namespace Tasks.Api.Tests.integration {
    public class TaskStatusIntegrationTests : IClassFixture<TasksApiFactory> {
        private readonly System.Net.Http.HttpClient _client;
        private readonly TasksApiFactory _factory;

        public TaskStatusIntegrationTests(TasksApiFactory factory) {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task PatchTask_ValidTransition_ToDoToInProgress_Returns200Ok() {
            // Arrange
            var projectId = Guid.NewGuid();
            var testTask = new TaskItem(projectId, "Test Task", "Desc", "Assignee", null);
            testTask.SetStatus(TaskItemStatus.ToDo);

            using (var scope = _factory.Services.CreateScope()) {
                var db = scope.ServiceProvider.GetRequiredService<TaskContext>();

                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                db.TaskItems.Add(testTask);
                await db.SaveChangesAsync();
            }

            // Setup mock project client
            _factory.ProjectClientMock!
                .Setup(c => c.GetProjectByIdAsync(projectId))
                .Returns(Task.FromResult(Result.Ok(new ProjectDTO(projectId, "Test Project", false))));

            var payload = new { status = "InProgress" };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PatchAsync($"/api/v1/projects/{projectId}/tasks/{testTask.id}/status", content);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseString = await response.Content.ReadAsStringAsync();
            var jsonNode = JsonNode.Parse(responseString);

            Assert.NotNull(jsonNode);

            // Читаємо статус як int і порівнюємо з числовим значенням enum
            var returnedStatusValue = (int)jsonNode["status"]!;
            Assert.Equal((int)TaskItemStatus.InProgress, returnedStatusValue);

            var updatedAtString = jsonNode["updatedAt"]?.ToString();
            Assert.NotNull(updatedAtString);

            var returnedUpdatedAt = DateTime.Parse(updatedAtString);
            Assert.True(returnedUpdatedAt > testTask.createdAt);
        }

        [Fact]
        public async Task PatchTask_InvalidTransition_ToDoToDone_Returns409Conflict() {
            // Arrange
            var projectId = Guid.NewGuid();
            var testTask = new TaskItem(projectId, "Test Task", "Desc", "Assignee", null);
            testTask.SetStatus(TaskItemStatus.ToDo);

            using (var scope = _factory.Services.CreateScope()) {
                var db = scope.ServiceProvider.GetRequiredService<TaskContext>();

                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                db.TaskItems.Add(testTask);
                await db.SaveChangesAsync();
            }

            // Setup mock project client
            _factory.ProjectClientMock!
                .Setup(c => c.GetProjectByIdAsync(projectId))
                .Returns(Task.FromResult(Result.Ok(new ProjectDTO(projectId, "Test Project", false))));

            var payload = new { status = "Done" };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PatchAsync($"/api/v1/projects/{projectId}/tasks/{testTask.id}/status", content);

            // Assert
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

            var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            Assert.NotNull(problemDetails);
            Assert.Equal(409, problemDetails.Status);
            Assert.Equal("Conflict", problemDetails.Title);
        }

        [Fact]
        public async Task PatchTask_InvalidStatusValue_Returns400BadRequest() {
            // Arrange
            var projectId = Guid.NewGuid();
            var taskId = Guid.NewGuid();

            using (var scope = _factory.Services.CreateScope()) {
                var db = scope.ServiceProvider.GetRequiredService<TaskContext>();
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
            }

            // Передаємо неіснуюче значення для enum TaskItemStatus
            var payload = new { status = "InvalidStatus" };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PatchAsync($"/api/v1/projects/{projectId}/tasks/{taskId}/status", content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            Assert.NotNull(problemDetails);
            Assert.Equal(400, problemDetails.Status);
            // Перевіряємо, що помилка валідації стосується поля Status
            Assert.True(problemDetails.Errors.ContainsKey("status") || problemDetails.Errors.ContainsKey("Status"));
        }

        [Fact]
        public async Task PatchTask_TaskNotFound_Returns404NotFound() {
            // Arrange
            var projectId = Guid.NewGuid();
            var nonExistentTaskId = Guid.NewGuid();

            using (var scope = _factory.Services.CreateScope()) {
                var db = scope.ServiceProvider.GetRequiredService<TaskContext>();
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
                // Завдання не додаємо
            }

            // Setup mock project client
            _factory.ProjectClientMock!
                .Setup(c => c.GetProjectByIdAsync(projectId))
                .Returns(Task.FromResult(Result.Ok(new ProjectDTO(projectId, "Test Project", false))));

            var payload = new { status = "InProgress" };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PatchAsync($"/api/v1/projects/{projectId}/tasks/{nonExistentTaskId}/status", content);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            Assert.NotNull(problemDetails);
            Assert.Equal(404, problemDetails.Status);
            Assert.Equal("Resource not found", problemDetails.Title);
        }

        [Fact]
        public async Task PatchTask_ProjectApiBadGateway_Returns502BadGateway() {
            // Arrange
            var projectId = Guid.NewGuid();
            var taskId = Guid.NewGuid();

            using (var scope = _factory.Services.CreateScope()) {
                var db = scope.ServiceProvider.GetRequiredService<TaskContext>();
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
            }

            // Setup mock project client to return Bad Gateway
            _factory.ProjectClientMock!
                .Setup(c => c.GetProjectByIdAsync(projectId))
                .Returns(Task.FromResult(Result.Fail<ProjectDTO>(new BadGatewayError("Projects API is down"))));

            var payload = new { status = "InProgress" };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PatchAsync($"/api/v1/projects/{projectId}/tasks/{taskId}/status", content);

            // Assert
            Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);

            var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();
            Assert.NotNull(problemDetails);
            Assert.Equal(502, problemDetails.Status);
            Assert.Equal("BadGateway", problemDetails.Title);
        }
    }
}