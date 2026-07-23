using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Tasks.Api.Data;
using Tasks.Api.Models;
using Tasks.Api.Services;
using Xunit;
using FluentResults;

namespace Tasks.Api.Tests.integration {
    public class UpdateTaskTests : IClassFixture<TasksApiFactory> {
        private readonly System.Net.Http.HttpClient _client;
        private readonly TasksApiFactory _factory;

        public UpdateTaskTests(TasksApiFactory factory) {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task PutTask_ValidData_ReturnOkWithUpdatedFields() {
            // Arrange
            var projectId = Guid.NewGuid();
            var testTask = new TaskItem(projectId, "Old Title", "Old Description", "Old Assignee", null);

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
            var requestDto = new TaskItemRequestDTO {
                title = "Updated Title",
                description = "Updated Description",
                assignee = "Updated Assignee",
                dueDate = DateTimeOffset.UtcNow.AddDays(5)
            };

            var response = await _client.PutAsJsonAsync($"/api/v1/projects/{projectId}/tasks/{testTask.id}", requestDto);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var returnedTask = await response.Content.ReadFromJsonAsync<TaskItem>();

            Assert.NotNull(returnedTask);
            Assert.Equal("Updated Title", returnedTask.title);
            Assert.Equal("Updated Description", returnedTask.description);
            Assert.Equal("Updated Assignee", returnedTask.assignee);
            Assert.NotNull(returnedTask.dueDate);
        }

        [Fact]
        public async Task PutTask_TaskDoesNotExist_ReturnNotFound() {
            // Arrange
            var projectId = Guid.NewGuid();
            var nonExistentTaskId = Guid.NewGuid();

            using (var scope = _factory.Services.CreateScope()) {
                var db = scope.ServiceProvider.GetRequiredService<TaskContext>();
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
            }

            // Setup mock project client
            _factory.ProjectClientMock!
                .Setup(c => c.GetProjectByIdAsync(projectId))
                .Returns(Task.FromResult(Result.Ok(new ProjectDTO(projectId, "Test Project", false))));

            // Act
            var requestDto = new TaskItemRequestDTO { title = "Updated Title" };
            var response = await _client.PutAsJsonAsync($"/api/v1/projects/{projectId}/tasks/{nonExistentTaskId}", requestDto);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();

            Assert.NotNull(problemDetails);
            Assert.Equal(404, problemDetails.Status);
            Assert.Equal("Resource not found", problemDetails.Title);
            Assert.NotNull(problemDetails.Detail);
        }

        [Fact]
        public async Task PutTask_EmptyTitle_ReturnBadRequest() {
            // Arrange
            var projectId = Guid.NewGuid();
            var testTask = new TaskItem(projectId, "Original Title", "Description", null, null);

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
            var requestDto = new TaskItemRequestDTO { title = "" };
            var response = await _client.PutAsJsonAsync($"/api/v1/projects/{projectId}/tasks/{testTask.id}", requestDto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            Assert.NotNull(problemDetails);
            Assert.Equal(400, problemDetails.Status);
            Assert.True(problemDetails.Errors.ContainsKey("title") || problemDetails.Errors.ContainsKey("Title"));
        }

        [Fact]
        public async Task PutTask_WhitespaceTitle_ReturnBadRequest() {
            // Arrange
            var projectId = Guid.NewGuid();
            var testTask = new TaskItem(projectId, "Original Title", "Description", null, null);

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
            var requestDto = new TaskItemRequestDTO { title = "   " };
            var response = await _client.PutAsJsonAsync($"/api/v1/projects/{projectId}/tasks/{testTask.id}", requestDto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PutTask_TitleTooLong_ReturnBadRequest() {
            // Arrange
            var projectId = Guid.NewGuid();
            var testTask = new TaskItem(projectId, "Original Title", "Description", null, null);

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
            string tooLongTitle = new string('a', 201);
            var requestDto = new TaskItemRequestDTO { title = tooLongTitle };
            var response = await _client.PutAsJsonAsync($"/api/v1/projects/{projectId}/tasks/{testTask.id}", requestDto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            Assert.NotNull(problemDetails);
            Assert.Equal(400, problemDetails.Status);
            Assert.True(problemDetails.Errors.ContainsKey("title") || problemDetails.Errors.ContainsKey("Title"));
        }

        [Fact]
        public async Task PutTask_DescriptionTooLong_ReturnBadRequest() {
            // Arrange
            var projectId = Guid.NewGuid();
            var testTask = new TaskItem(projectId, "Original Title", "Description", null, null);

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
            string tooLongDescription = new string('b', 2001);
            var requestDto = new TaskItemRequestDTO { 
                title = "Valid Title",
                description = tooLongDescription 
            };
            var response = await _client.PutAsJsonAsync($"/api/v1/projects/{projectId}/tasks/{testTask.id}", requestDto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            Assert.NotNull(problemDetails);
            Assert.Equal(400, problemDetails.Status);
            Assert.True(problemDetails.Errors.ContainsKey("description") || problemDetails.Errors.ContainsKey("Description"));
        }

        [Fact]
        public async Task PutTask_AssigneeTooLong_ReturnBadRequest() {
            // Arrange
            var projectId = Guid.NewGuid();
            var testTask = new TaskItem(projectId, "Original Title", "Description", null, null);

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
            string tooLongAssignee = new string('c', 201);
            var requestDto = new TaskItemRequestDTO { 
                title = "Valid Title",
                assignee = tooLongAssignee 
            };
            var response = await _client.PutAsJsonAsync($"/api/v1/projects/{projectId}/tasks/{testTask.id}", requestDto);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            Assert.NotNull(problemDetails);
            Assert.Equal(400, problemDetails.Status);
            Assert.True(problemDetails.Errors.ContainsKey("assignee") || problemDetails.Errors.ContainsKey("Assignee"));
        }
       
    }
}
