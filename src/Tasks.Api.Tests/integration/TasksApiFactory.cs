using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Linq;
using Tasks.Api.Data;
using Tasks.Api.Services;

namespace Tasks.Api.Tests.integration {
    public class TasksApiFactory : WebApplicationFactory<Program> {
        public Mock<IProjectClient>? ProjectClientMock { get; private set; }

        protected override void ConfigureWebHost(IWebHostBuilder builder) {
            builder.ConfigureServices(services => {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<TaskContext>));

                if (descriptor != null) {
                    services.Remove(descriptor);
                }

                string dbName = $"InMemoryDbForTesting_{Guid.NewGuid()}";
                services.AddDbContext<TaskContext>(options => {
                    options.UseInMemoryDatabase(dbName);
                });

                // Mock the project client for testing
                var projectClientDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IProjectClient));
                
                if (projectClientDescriptor != null) {
                    services.Remove(projectClientDescriptor);
                }

                ProjectClientMock = new Mock<IProjectClient>();
                services.AddScoped<IProjectClient>(sp => ProjectClientMock.Object);
            });
        }
    }
}
