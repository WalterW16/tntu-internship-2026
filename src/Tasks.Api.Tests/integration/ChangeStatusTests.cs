using FluentResults;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Tasks.Api.Data;
using Tasks.Api.Errors;
using Tasks.Api.Models;
using Tasks.Api.Models.Tasks.Api.Models; // Згідно з вашим неймспейсом
using Tasks.Api.Services;
using Xunit;
using System.Text.Json.Nodes; // Необхідно для JsonNode


namespace Tasks.Api.Tests.integration {
    public class TaskStatusIntegrationTests : IClassFixture<WebApplicationFactory<Program>> {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly Mock<IProjectClient> _mockProjectClient;

        public TaskStatusIntegrationTests(WebApplicationFactory<Program> factory) {
            _mockProjectClient = new Mock<IProjectClient>();

            // 1. Створюємо унікальне ім'я БД ОДИН РАЗ для поточного екземпляра тесту
            var dbName = "IntegrationTestsDb_" + Guid.NewGuid().ToString();

            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    // Видаляємо існуючі налаштування DbContext
                    services.RemoveAll(typeof(DbContextOptions<TaskContext>));

                    // 2. Використовуємо згенероване ім'я
                    services.AddDbContext<TaskContext>(options =>
                    {
                        options.UseInMemoryDatabase(dbName);
                    });

                    services.RemoveAll(typeof(IProjectClient));
                    services.AddSingleton(_mockProjectClient.Object);
                });
            });
        }

        private async Task<TaskItem> SeedTaskAsync(Guid projectId, TaskItemStatus initialStatus) {
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<TaskContext>();

            var task = new TaskItem(projectId, "Test Task", "Desc", "Assignee", null);
            task.setStatus(initialStatus);

            context.TaskItems.Add(task);
            await context.SaveChangesAsync();

            return task;
        }
        [Fact]
        public async Task PatchTask_ValidTransition_ToDoToInProgress_Returns200Ok() {
            // Arrange
            var projectId = Guid.NewGuid();
            var task = await SeedTaskAsync(projectId, TaskItemStatus.ToDo);

            _mockProjectClient
                .Setup(c => c.GetProjectByIdAsync(projectId))
                .ReturnsAsync(Result.Ok(new ProjectDTO(projectId, "Test Project", false)));

            var client = _factory.CreateClient();

            var payload = new { status = "InProgress" };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            // Act
            var response = await client.PatchAsync($"/api/v1/projects/{projectId}/tasks/{task.id}/status", content);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseString = await response.Content.ReadAsStringAsync();
            var jsonNode = JsonNode.Parse(responseString);
            Assert.NotNull(jsonNode);

            // ВИПРАВЛЕННЯ: Читаємо статус як int і порівнюємо з (int)TaskItemStatus.InProgress
            var returnedStatusValue = (int)jsonNode["status"];
            Assert.Equal((int)TaskItemStatus.InProgress, returnedStatusValue);

            var updatedAtString = jsonNode["updatedAt"]?.ToString();
            Assert.NotNull(updatedAtString);

            var returnedUpdatedAt = DateTime.Parse(updatedAtString);
            Assert.True(returnedUpdatedAt > task.createdAt);
        }

        [Fact]
        public async Task PatchTask_InvalidTransition_ToDoToDone_Returns409Conflict() {
            // Arrange
            var projectId = Guid.NewGuid();
            var task = await SeedTaskAsync(projectId, TaskItemStatus.ToDo);

            _mockProjectClient
                .Setup(c => c.GetProjectByIdAsync(projectId))
                .ReturnsAsync(Result.Ok(new ProjectDTO(projectId, "Test Project", false)));

            var client = _factory.CreateClient();
            var payload = new { status = "Done" }; // Спроба перескочити InProgress[cite: 1]
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            // Act
            var response = await client.PatchAsync($"/api/v1/projects/{projectId}/tasks/{task.id}/status", content);

            // Assert
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode); // Перевіряємо 409 Conflict[cite: 1]
        }

        [Fact]
        public async Task PatchTask_InvalidStatusValue_Returns400BadRequest() {
            // Arrange
            var projectId = Guid.NewGuid();
            var taskId = Guid.NewGuid();
            var client = _factory.CreateClient();

            // Передаємо неіснуюче значення для enum TaskItemStatus
            var payload = new { status = "InvalidStatus" };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            // Act
            var response = await client.PatchAsync($"/api/v1/projects/{projectId}/tasks/{taskId}/status", content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode); // Перевіряємо 400 Bad Request[cite: 1]
        }

        [Fact]
        public async Task PatchTask_TaskNotFound_Returns404NotFound() {
            // Arrange
            var projectId = Guid.NewGuid();
            var nonExistentTaskId = Guid.NewGuid();

            _mockProjectClient
                .Setup(c => c.GetProjectByIdAsync(projectId))
                .ReturnsAsync(Result.Ok(new ProjectDTO(projectId, "Test Project", false)));

            var client = _factory.CreateClient();
            var payload = new { status = "InProgress" };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            // Act
            var response = await client.PatchAsync($"/api/v1/projects/{projectId}/tasks/{nonExistentTaskId}/status", content);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task PatchTask_ProjectApiBadGateway_Returns502BadGateway() {
            // Arrange
            var projectId = Guid.NewGuid();
            var taskId = Guid.NewGuid();

            _mockProjectClient
                .Setup(c => c.GetProjectByIdAsync(projectId))
                .ReturnsAsync(Result.Fail(new BadGatewayError("API is down")));

            var client = _factory.CreateClient();
            var payload = new { status = "InProgress" };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            // Act
            var response = await client.PatchAsync($"/api/v1/projects/{projectId}/tasks/{taskId}/status", content);

            // Assert
            Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
        }
    }
}