using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Projects.Api.Data;
using Projects.Api.Models;
using Xunit;

namespace Projects.Api.Tests.integration {
    public class UpdateProjectTests : IClassFixture<ProjectsApiFactory> {
        private readonly System.Net.Http.HttpClient _client;
        private readonly ProjectsApiFactory _factory;

        public UpdateProjectTests(ProjectsApiFactory factory) {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task PutProject_ValidData_ReturnOkAndUpdatedProject() {
            Project testProject = new Project("Old Name", "Old Description");


            using (var scope = _factory.Services.CreateScope()) {
                var db = scope.ServiceProvider.GetRequiredService<ProjectContext>();
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                db.Projects.Add(testProject);
                await db.SaveChangesAsync();
            }

            var requestDto = new ProjectRequestDTO();
            requestDto.name = "Updated Name";
            requestDto.description = "Updated Description";

            var response = await _client.PutAsJsonAsync($"/api/v1/projects/{testProject.id}", requestDto);

            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var returnedProject = await response.Content.ReadFromJsonAsync<Project>();

            Assert.NotNull(returnedProject);
            Assert.Equal(testProject.id, returnedProject.id);
            Assert.Equal("Updated Name", returnedProject.name);
            Assert.Equal("Updated Description", returnedProject.description);
        }

        [Fact]
        public async Task PutProject_SpecifiedIdDoesntExist_ReturnNotFound() {
            using (var scope = _factory.Services.CreateScope()) {
                var db = scope.ServiceProvider.GetRequiredService<ProjectContext>();
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
            }

            Guid nonExistentId = Guid.NewGuid();
            var requestDto = new ProjectRequestDTO();
            requestDto.name = "Name";
            requestDto.description = "Desc";

            var response = await _client.PutAsJsonAsync($"/api/v1/projects/{nonExistentId}", requestDto);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var errorResponse = await response.Content.ReadFromJsonAsync<TestErrorMessage>();
            Assert.NotNull(errorResponse);
            Assert.NotEmpty(errorResponse.message);
        }

        [Fact]
        public async Task PutProject_ProjectIsArchived_ReturnConflict() {
            Project testProject = new Project("Archived Project", "Description") { isArchived = true };

            using (var scope = _factory.Services.CreateScope()) {
                var db = scope.ServiceProvider.GetRequiredService<ProjectContext>();
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                db.Projects.Add(testProject);
                await db.SaveChangesAsync();
            }

            var requestDto = new ProjectRequestDTO();
            requestDto.name = "Try Update Name";
            requestDto.description = "Try Update Desc";
            
            var response = await _client.PutAsJsonAsync($"/api/v1/projects/{testProject.id}", requestDto);

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

            var errorResponse = await response.Content.ReadFromJsonAsync<TestErrorMessage>();
            Assert.NotNull(errorResponse);
            Assert.NotEmpty(errorResponse.message);
        }
        [Fact]
        public async Task PutProject_EmptyName_ReturnBadRequest() {
            var projectId = Guid.NewGuid();
            var requestDto = new ProjectRequestDTO { name = "", description = "Valid description" };

            var response = await _client.PutAsJsonAsync($"/api/v1/projects/{projectId}", requestDto);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            Assert.NotNull(problemDetails);
            Assert.Equal(400, problemDetails.Status);
            Assert.True(problemDetails.Errors.ContainsKey("name") || problemDetails.Errors.ContainsKey("Name"));
        }

        [Fact]
        public async Task PutProject_TooLongName_ReturnBadRequest() {
            var projectId = Guid.NewGuid();
            string tooLongName = new string('a', 101);
            var requestDto = new ProjectRequestDTO { name = tooLongName, description = "Valid description" };

            var response = await _client.PutAsJsonAsync($"/api/v1/projects/{projectId}", requestDto);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            Assert.NotNull(problemDetails);
            Assert.Equal(400, problemDetails.Status);
            Assert.True(problemDetails.Errors.ContainsKey("name") || problemDetails.Errors.ContainsKey("Name"));
        }

        [Fact]
        public async Task PutProject_TooLongDescription_ReturnBadRequest() {
            var projectId = Guid.NewGuid();
            string tooLongDescription = new string('b', 501);
            var requestDto = new ProjectRequestDTO { name = "Valid Name", description = tooLongDescription };

            var response = await _client.PutAsJsonAsync($"/api/v1/projects/{projectId}", requestDto);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            Assert.NotNull(problemDetails);
            Assert.Equal(400, problemDetails.Status);
            Assert.True(problemDetails.Errors.ContainsKey("description") || problemDetails.Errors.ContainsKey("Description"));
        }
    }
    public record TestErrorMessage(string message);
}