using Asp.Versioning;
using Microsoft.EntityFrameworkCore;
using Tasks.Api.Data;
using Tasks.Api.Services;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

var cosmosEndpoint = builder.Configuration["CosmosDb:Endpoint"];
var cosmosKey = builder.Configuration["CosmosDb:Key"];
var databaseName = builder.Configuration["CosmosDb:DatabaseName"];
var projectsApiBaseUrl = builder.Configuration["ProjectsApi:BaseUrl"];

// Add services to the container.
builder.Services.AddDbContext<TaskContext>(opt =>
    opt.UseCosmos(cosmosEndpoint, cosmosKey, databaseName));

builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddControllers();

builder.Services.AddHttpClient<IProjectClient, ProjectClient>(client => {
    client.BaseAddress = new Uri(projectsApiBaseUrl!);
});

builder.Services.AddHealthChecks()
    .AddDbContextCheck<TaskContext>(
        name: "cosmosdb",
        customTestQuery: async (context, cancellationToken) => {
            try {
                var cosmosClient = context.Database.GetCosmosClient();
                await cosmosClient.ReadAccountAsync();
                return true;
            } catch {
                return false;
            }
        })
    .AddUrlGroup(
        new Uri($"{projectsApiBaseUrl?.TrimEnd('/')}/health"),
        name: "projects_api",
        failureStatus: HealthStatus.Degraded);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddSwaggerGen();
builder.Services.AddApiVersioning(options => {
    options.DefaultApiVersion = new ApiVersion(1);
    options.ReportApiVersions = true;
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("Tasks-Api-Version")); 
})
.AddMvc() // This is needed for controllers
.AddApiExplorer(options => {
    options.GroupNameFormat = "'v'V";
    options.SubstituteApiVersionInUrl = true;
});

var app = builder.Build();

using (var scope = app.Services.CreateScope()) {
    var dbContext = scope.ServiceProvider.GetRequiredService<TaskContext>();
    dbContext.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// US-012: Мапінг ендпоінту та кастомний JSON-форматер
app.MapHealthChecks("/health", new HealthCheckOptions {
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