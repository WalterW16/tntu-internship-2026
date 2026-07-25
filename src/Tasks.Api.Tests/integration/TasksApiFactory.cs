using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Tasks.Api.Data;
using Tasks.Api.Services;

namespace Tasks.Api.Tests.integration {
    public class TasksApiFactory : WebApplicationFactory<Program> {
        public Mock<IProjectClient>? ProjectClientMock { get; private set; }

        protected override void ConfigureWebHost(IWebHostBuilder builder) {

            builder.ConfigureAppConfiguration((context, config) => {
                _ = config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "APPLICATIONINSIGHTS_CONNECTION_STRING", "InstrumentationKey=00000000-0000-0000-0000-000000000000;" }
                });
            });

            builder.ConfigureServices(services => {
                services.Configure<TelemetryConfiguration>(config => {
                    config.DisableTelemetry = true;
                });
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<TaskContext>));

                if (descriptor != null) {
                    services.Remove(descriptor);
                }

                string dbName = $"InMemoryDbForTesting_{Guid.NewGuid()}";
                services.AddDbContext<TaskContext>(options => {
                    options.UseInMemoryDatabase(dbName);
                });

                // === Налаштування Mock для IProjectClient ===
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