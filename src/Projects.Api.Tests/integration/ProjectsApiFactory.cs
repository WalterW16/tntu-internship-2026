using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Projects.Api.Data;
using System;
using System.Linq;

namespace Projects.Api.Tests.integration {
    public class ProjectsApiFactory : WebApplicationFactory<Program> {
        protected override void ConfigureWebHost(IWebHostBuilder builder) {
            builder.ConfigureServices(services => {
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