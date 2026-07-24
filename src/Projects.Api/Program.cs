using Asp.Versioning;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Cosmos;
using Projects.Api.Data;
using Projects.Api.Services;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("ProjectContext") ?? throw new InvalidOperationException("Connection string 'ProjectContext' not found.");

var cosmosEndpoint = builder.Configuration["CosmosDb:Endpoint"];
var cosmosKey = builder.Configuration["CosmosDb:Key"];
var databaseName = builder.Configuration["CosmosDb:DatabaseName"];

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddSwaggerGen();
builder.Services.AddApiVersioning(options => {
    options.DefaultApiVersion = new ApiVersion(1);
    options.ReportApiVersions = true;
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("Projects-Api-Version"));
})
.AddMvc() // This is needed for controllers
.AddApiExplorer(options => {
    options.GroupNameFormat = "'v'V";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddDbContext<ProjectContext>(opt =>
opt.UseCosmos(cosmosEndpoint, cosmosKey, databaseName));
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ProjectContext>( 
        name: "cosmosdb",
        customTestQuery: async (context, cancellationToken) => {
            try {
                var cosmosClient = context.Database.GetCosmosClient();
                await cosmosClient.ReadAccountAsync();
                return true; 
            } catch {
                return false; 
            }
        }); builder.Services.AddScoped<IProjectService, ProjectsService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope()) {
    var services = scope.ServiceProvider;
    try {
        var context = services.GetRequiredService<ProjectContext>();
        await context.Database.EnsureCreatedAsync();
        Console.WriteLine("Success, connected to Cosmos DB!");
    } catch (Exception ex) {
        Console.WriteLine($" Error inializing from the start: {ex.Message}");
    }
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions {
    ResponseWriter = async (context, report) => {
        context.Response.ContentType = "application/json";
        var response = new {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new {
                name = e.Key,
                status = e.Value.Status.ToString()
            })
        };
        await JsonSerializer.SerializeAsync(context.Response.Body, response);
    }
});
app.Run();
public partial class Program { }
