using Microsoft.Extensions.DependencyInjection;
using Projects.Api.Data;
using Projects.Api.Models;
using Projects.Api.Tests.integration;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace Projects.Api.Tests.integration {
   
    public class GetNonArchivedProjectsTests : IClassFixture<ProjectsApiFactory> {
        private readonly System.Net.Http.HttpClient _client;
        private readonly ProjectsApiFactory _factory;

        public GetNonArchivedProjectsTests(ProjectsApiFactory factory) {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task GetNonArchivedProjects_ProjectsExist_ReturnOkAndArrayOfProject() {
           
            using (var scope = _factory.Services.CreateScope()) {
                var db = scope.ServiceProvider.GetRequiredService<ProjectContext>();
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                
                db.Projects.Add(new Project("Active Project 1", "Desc") { isArchived = false });
                db.Projects.Add(new Project("Active Project 2", "Desc") { isArchived = false });
                db.Projects.Add(new Project("Archived Project", "Desc") { isArchived = true });

                await db.SaveChangesAsync();
            }

           var response = await _client.GetAsync("/api/v1/projects");

            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var projects = await response.Content.ReadFromJsonAsync<List<Project>>();

            Assert.NotNull(projects);
            Assert.Equal(2, projects.Count); 
            Assert.DoesNotContain(projects, p => p.isArchived);
        }

        [Fact]
        public async Task GetNonArchivedProjects_ProjectsDontExist_ReturnOkAndEmptyArray() {
            
            using (var scope = _factory.Services.CreateScope()) {
                var db = scope.ServiceProvider.GetRequiredService<ProjectContext>();
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
            }

            var response = await _client.GetAsync("/api/v1/projects");

            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var projects = await response.Content.ReadFromJsonAsync<List<Project>>();

            Assert.NotNull(projects);
            Assert.Empty(projects);
        }
    }
}