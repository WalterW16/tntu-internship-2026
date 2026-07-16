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
    public class ArchiveProjectTests : IClassFixture<ProjectsApiFactory> {
        private readonly System.Net.Http.HttpClient _client;
        private readonly ProjectsApiFactory _factory;

        public ArchiveProjectTests(ProjectsApiFactory factory) {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task ArchiveProject_ValidData_ReturnOkAndUpdatedProject() {
            Project testProject = new Project("Name", "Description");

            using (var scope = _factory.Services.CreateScope()) {
                var db = scope.ServiceProvider.GetRequiredService<ProjectContext>();
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                db.Projects.Add(testProject);
                await db.SaveChangesAsync();
            }
            var response = await _client.PatchAsync($"/api/v1/projects/{testProject.id}/archive", null);

            response.EnsureSuccessStatusCode();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var returnedProject = await response.Content.ReadFromJsonAsync<Project>();
            Assert.NotNull(returnedProject);
            Assert.Equal(testProject.id, returnedProject.id);
            Assert.Equal(true, returnedProject.isArchived);
        }

        [Fact]
        public async Task ArchiveProject_SpecifiedIdDoesntExist_ReturnNotFound() {
            using (var scope = _factory.Services.CreateScope()) {
                var db = scope.ServiceProvider.GetRequiredService<ProjectContext>();
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
            }

            Guid nonExistentId = Guid.NewGuid();
            var requestDto = new ProjectRequestDTO {
                name = "Name",
                description = "Desc"
            };

            var response = await _client.PatchAsync($"/api/v1/projects/{nonExistentId}/archive", null);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();

            Assert.NotNull(problemDetails);
            Assert.Equal(404, problemDetails.Status);
            Assert.Equal("Project not found", problemDetails.Title);
            Assert.NotEmpty(problemDetails.Detail);
        }
        [Fact]
        public async Task ArchiveProject_ProjectIsArchived_ReturnConflict() {
            Project testProject = new Project("Archived Project", "Description");
            testProject.isArchived = true;
            using (var scope = _factory.Services.CreateScope()) {
                var db = scope.ServiceProvider.GetRequiredService<ProjectContext>();
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                db.Projects.Add(testProject);
                await db.SaveChangesAsync();
            }

            var response = await _client.PatchAsync($"/api/v1/projects/{testProject.id}/archive", null);
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

            var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();

            Assert.NotNull(problemDetails);
            Assert.Equal(409, problemDetails.Status);
            Assert.Equal("Conflict", problemDetails.Title);
            Assert.NotEmpty(problemDetails.Detail);
        }
  
    }
}
