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
    public class GetProjectByIdTests : IClassFixture<ProjectsApiFactory> {
        private readonly System.Net.Http.HttpClient _client;
        private readonly ProjectsApiFactory _factory;

        public GetProjectByIdTests(ProjectsApiFactory factory) {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task GetProjectById_InvalidId_ReturnBadRequest() {
            var response = await _client.GetAsync("/api/v1/projects/invalid-guid-format");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            Assert.NotNull(problemDetails);
            Assert.Equal(400, problemDetails.Status);
            Assert.NotEmpty(problemDetails.Errors); 
        }

        [Fact]
        public async Task GetProjectById_ValidId_ReturnOkAndProject() {
            Project testProject = new Project("Test Project By Id", "Description") { isArchived = true };

            using (var scope = _factory.Services.CreateScope()) {
                var db = scope.ServiceProvider.GetRequiredService<ProjectContext>();
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                db.Projects.Add(testProject);
                await db.SaveChangesAsync();
            }

           
            var response = await _client.GetAsync($"/api/v1/projects/{testProject.id}");

          
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var returnedProject = await response.Content.ReadFromJsonAsync<TestProjectResponse>();

            Assert.NotNull(returnedProject);
            Assert.Equal(testProject.id, returnedProject.id); 
            Assert.True(returnedProject.IsArchived);
        }

        [Fact]
        public async Task GetProjectById_SpecifiedIdDoesntExist_ReturnNotFoundAndProblemDetails() {
            using (var scope = _factory.Services.CreateScope()) {
                var db = scope.ServiceProvider.GetRequiredService<ProjectContext>();
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
            }

            Guid nonExistentId = Guid.NewGuid();

            var response = await _client.GetAsync($"/api/v1/projects/{nonExistentId}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            var problemDetails = await response.Content.ReadFromJsonAsync<ProblemDetails>();

            Assert.NotNull(problemDetails);
            Assert.Equal(404, problemDetails.Status);
            Assert.Equal("Project not found", problemDetails.Title);
            Assert.Contains(nonExistentId.ToString(), problemDetails.Detail); 
        }
    }  
    public record TestProjectResponse(Guid id, string Name, string Description, bool IsArchived);
}