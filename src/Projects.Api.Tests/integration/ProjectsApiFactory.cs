using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Projects.Api.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Projects.Api.Tests.integration {
    public class ProjectsApiFactory : WebApplicationFactory<Program> {
        protected override void ConfigureWebHost(IWebHostBuilder builder) {

            builder.ConfigureAppConfiguration((context, config) => {
                // Додано знак питання до string? щоб уникнути попередження CS8620
                _ = config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "APPLICATIONINSIGHTS_CONNECTION_STRING", "InstrumentationKey=00000000-0000-0000-0000-000000000000;" }
                });
            });
            builder.ConfigureServices(services => {
                services.Configure<Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration>(config => {
                    config.DisableTelemetry = true;
                });
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ProjectContext>));
                if (descriptor != null) {
                    services.Remove(descriptor);
                }
                string dbName = $"InMemoryDbForTesting_{Guid.NewGuid()}";
                services.AddDbContext<ProjectContext>(options => {
                    options.UseInMemoryDatabase(dbName);
                });
            });
        }
    }
}