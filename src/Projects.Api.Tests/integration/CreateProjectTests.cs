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
    public class CreateProjectTests : IClassFixture<ProjectsApiFactory> {
        private readonly System.Net.Http.HttpClient _client;
        private readonly ProjectsApiFactory _factory;

        public CreateProjectTests(ProjectsApiFactory factory) {
            _factory = factory;
            _client = _factory.CreateClient();
        }
        [Fact]
        public async Task CreateProject_ValidData_ReturnOkAndCreatedProject() {
            var requestDto = new ProjectRequestDTO { name = "Valid name", description = "Valid description" };

            var response = await _client.PostAsJsonAsync($"/api/v1/projects", requestDto);

            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var returntedProject = await response.Content.ReadFromJsonAsync<Project>();

            Assert.NotNull(returntedProject);
            Assert.Equal(false, returntedProject.isArchived);
            Assert.Equal(requestDto.name, returntedProject.name);
            Assert.Equal(requestDto.description, returntedProject.description);
        }
        [Fact]
        public async Task CreateProject_EmptyName_ReturnBadRequest() {
            var requestDto = new ProjectRequestDTO { name = "", description = "Valid description" };

            var response = await _client.PostAsJsonAsync($"/api/v1/projects", requestDto);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            Assert.NotNull(problemDetails);
            Assert.Equal(400, problemDetails.Status);
            Assert.True(problemDetails.Errors.ContainsKey("name") || problemDetails.Errors.ContainsKey("Name"));
        }
        [Fact]
        public async Task CreateProject_TooLongName_ReturnBadRequest() {
            
            string tooLongName = new string('a', 101);
            var requestDto = new ProjectRequestDTO { name = tooLongName, description = "Valid description" };

            var response = await _client.PostAsJsonAsync($"/api/v1/projects", requestDto);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            Assert.NotNull(problemDetails);
            Assert.Equal(400, problemDetails.Status);
            Assert.True(problemDetails.Errors.ContainsKey("name") || problemDetails.Errors.ContainsKey("Name"));
        }
        [Fact]
        public async Task CreateProject_TooLongDescription_ReturnBadRequest() {
          
            string tooLongDescription = new string('b', 501);
            var requestDto = new ProjectRequestDTO { name = "Valid Name", description = tooLongDescription };

            var response = await _client.PostAsJsonAsync($"/api/v1/projects", requestDto);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

            Assert.NotNull(problemDetails);
            Assert.Equal(400, problemDetails.Status);
            Assert.True(problemDetails.Errors.ContainsKey("description") || problemDetails.Errors.ContainsKey("Description"));
        }
    }
}
