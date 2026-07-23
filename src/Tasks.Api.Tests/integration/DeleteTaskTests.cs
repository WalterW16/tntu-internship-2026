using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Tasks.Api.Data;
using Tasks.Api.Errors;
using Tasks.Api.Models;
using Tasks.Api.Services;
using Xunit;
using FluentResults;

namespace Tasks.Api.Tests.integration {
    public class DeleteTaskTests : IClassFixture<TasksApiFactory> {
        private readonly System.Net.Http.HttpClient _client;
        private readonly TasksApiFactory _factory;

        public DeleteTaskTests(TasksApiFactory factory) {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task DeleteTask_WhenTaskExists_ReturnsNoContent() {
            // Arrange
            var projectId = Guid.NewGuid();
            var testTask = new TaskItem(projectId, "Task to delete", "Description", "Assignee", null);

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

            // Act
            var response = await _client.DeleteAsync($"/api/v1/projects/{projectId}/tasks/{testTask.id}");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Verify task was actually deleted from the database
            using (var scope = _factory.Services.CreateScope()) {
                var db = scope.ServiceProvider.GetRequiredService<TaskContext>();
                var deletedTask = await db.TaskItems.FindAsync(testTask.id);
                Assert.Null(deletedTask);
            }
        }

        [Fact]
        public async Task DeleteTask_WhenTaskDoesNotExist_ReturnsNotFound() {
            // Arrange
            var projectId = Guid.NewGuid();
            var nonExistentTaskId = Guid.NewGuid();

            using (var scope = _factory.Services.CreateScope()) {
                var db = scope.ServiceProvider.GetRequiredService<TaskContext>();

                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
                // We don't add any tasks here
            }

            // Setup mock project client
            _factory.ProjectClientMock!
                .Setup(c => c.GetProjectByIdAsync(projectId))
                .Returns(Task.FromResult(Result.Ok(new ProjectDTO(projectId, "Test Project", false))));

            // Act
            var response = await _client.DeleteAsync($"/api/v1/projects/{projectId}/tasks/{nonExistentTaskId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();

            Assert.NotNull(problemDetails);
            Assert.Equal(404, problemDetails.Status);
            Assert.Equal("Resource not found", problemDetails.Title);
            Assert.Equal("No task with specified id", problemDetails.Detail);
        }

        [Fact]
        public async Task DeleteTask_WhenProjectDoesNotExist_ReturnsNotFound() {
            // Arrange
            var projectId = Guid.NewGuid();
            var taskId = Guid.NewGuid();

            using (var scope = _factory.Services.CreateScope()) {
                var db = scope.ServiceProvider.GetRequiredService<TaskContext>();
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
            }

            // Setup mock project client to return NotFoundError
            _factory.ProjectClientMock!
                .Setup(c => c.GetProjectByIdAsync(projectId))
                .Returns(Task.FromResult(Result.Fail<ProjectDTO>(new NotFoundError("Project not found"))));

            // Act
            var response = await _client.DeleteAsync($"/api/v1/projects/{projectId}/tasks/{taskId}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();

            Assert.NotNull(problemDetails);
            Assert.Equal(404, problemDetails.Status);
            Assert.Equal("Resource not found", problemDetails.Title);
            Assert.Equal("Project not found", problemDetails.Detail);
        }

        [Fact]
        public async Task DeleteTask_WhenProjectApiUnavailable_ReturnsBadGateway() {
            // Arrange
            var projectId = Guid.NewGuid();
            var taskId = Guid.NewGuid();

            using (var scope = _factory.Services.CreateScope()) {
                var db = scope.ServiceProvider.GetRequiredService<TaskContext>();
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
            }

            // Setup mock project client to return BadGatewayError
            _factory.ProjectClientMock!
                .Setup(c => c.GetProjectByIdAsync(projectId))
                .Returns(Task.FromResult(Result.Fail<ProjectDTO>(new BadGatewayError("Projects API is down"))));

            // Act
            var response = await _client.DeleteAsync($"/api/v1/projects/{projectId}/tasks/{taskId}");

            // Assert
            Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
            var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();

            Assert.NotNull(problemDetails);
            Assert.Equal(502, problemDetails.Status);
            Assert.Equal("BadGateway", problemDetails.Title);
            Assert.Equal("Projects API is down", problemDetails.Detail);
        }
    }
}